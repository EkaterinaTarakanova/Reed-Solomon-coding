namespace ReedSolomon;

public class ReedSolomon<T>
{
    private readonly IField<T> field;
    private readonly T generator;
    private readonly int messageLen;
    private readonly int eccLen;
    private readonly int codewordLen;

    public ReedSolomon(IField<T> field, T gen, int msglen, int ecclen)
    {
        if (field == null || gen == null)
            throw new ArgumentNullException();
        if (msglen <= 0 || ecclen <= 0)
            throw new ArgumentException("Invalid message or ECC length");
        
        this.field = field;
        this.generator = gen;
        this.messageLen = msglen;
        this.eccLen = ecclen;
        this.codewordLen = msglen + ecclen;
    }

    public T[] Encode(T[] message)
    {
        if (message.Length != messageLen)
            throw new ArgumentException("Invalid message length");
        
        T[] genpoly = MakeGeneratorPolynomial();
        T[] eccpoly = new T[eccLen];
        Array.Fill(eccpoly, field.Zero());

        for (int i = messageLen - 1; i >= 0; i--)
        {
            T factor = field.Add(message[i], eccpoly[eccLen - 1]);
            
            // Shift coefficients right with zero padding
            for (int j = eccLen - 1; j > 0; j--)
                eccpoly[j] = eccpoly[j - 1];
            eccpoly[0] = field.Zero();
            
            for (int j = 0; j < eccLen; j++)
                eccpoly[j] = field.Subtract(eccpoly[j], field.Multiply(genpoly[j], factor));
        }

        // Negate ECC values and combine with message
        T[] eccNeg = eccpoly.Select(val => field.Negate(val)).ToArray();
        return eccNeg.Concat(message).ToArray();
    }

    private T[] MakeGeneratorPolynomial()
    {
        T[] result = new T[eccLen + 1];
        result[0] = field.One();
        for (int i = 1; i <= eccLen; i++)
            result[i] = field.Zero();
        
        T genpow = field.One();

        for (int i = 0; i < eccLen; i++)
        {
            // Multiply result by (x - genpow)
            for (int j = eccLen; j >= 1; j--)
            {
                result[j] = field.Multiply(field.Negate(genpow), result[j]);
                if (j > 0)
                    result[j] = field.Add(result[j - 1], result[j]);
            }
            result[0] = field.Multiply(field.Negate(genpow), result[0]);
            
            genpow = field.Multiply(generator, genpow);
        }
        
        // Trim leading zero coefficients
        return result.Take(eccLen + 1).ToArray();
    }

    public T[] Decode(T[] codeword, int? numerrorstocorrect = null)
    {
        if (codeword.Length != codewordLen)
            throw new ArgumentException("Invalid codeword length");
        
        int errorsToCorrect = numerrorstocorrect ?? eccLen / 2;
        if (errorsToCorrect < 0 || errorsToCorrect > eccLen / 2)
            throw new ArgumentException("Number of errors to correct is out of range");
        
        T[] syndromes = CalculateSyndromes(codeword);
        
        // Check if any syndrome is non-zero
        if (syndromes.Any(s => !field.Equals(s, field.Zero())))
        {
            if (errorsToCorrect == 0)
                return null;
            
            T[] errlocpoly = CalculateErrorLocatorPolynomial(syndromes, errorsToCorrect);
            if (errlocpoly == null)
                return null;
            
            List<int> errlocs = FindErrorLocations(errlocpoly, errorsToCorrect);
            if (errlocs == null || errlocs.Count == 0)
                return null;
            
            T[] errvals = CalculateErrorValues(errlocs, syndromes);
            if (errvals == null)
                return null;
            
            T[] newcodeword = FixErrors(codeword, errlocs, errvals);
            T[] newsyndromes = CalculateSyndromes(newcodeword);
            
            if (newsyndromes.Any(s => !field.Equals(s, field.Zero())))
                throw new InvalidOperationException("Failed to correct errors");
            
            codeword = newcodeword;
        }
        
        return codeword.Skip(eccLen).ToArray();
    }

    private T[] CalculateSyndromes(T[] codeword)
    {
        T[] syndromes = new T[eccLen];
        T genpow = field.One();
        
        for (int i = 0; i < eccLen; i++)
        {
            syndromes[i] = EvaluatePolynomial(codeword, genpow);
            genpow = field.Multiply(generator, genpow);
        }
        
        return syndromes;
    }

    public T[] CalculateErrorLocatorPolynomial(T[] syndromes, int numerrorstocorrect)
    {
        Matrix<T> matrix = new Matrix<T>(numerrorstocorrect, numerrorstocorrect + 1, field);
    
        // Заполнение матрицы синдромами
        for (int r = 0; r < numerrorstocorrect; r++)
        {
            for (int c = 0; c <= numerrorstocorrect; c++)
            {
                int index = r + c;
                T val = (index < syndromes.Length) ? syndromes[index] : field.Zero();
            
                if (c == numerrorstocorrect) // Последний столбец
                    val = field.Negate(val);
            
                matrix.Set(r, c, val);
            }
        }
    
        matrix.ReducedRowEchelonForm();
    
        // Инициализация полинома
        T[] result = new T[numerrorstocorrect + 1];
        result[0] = field.One();
        for (int i = 1; i <= numerrorstocorrect; i++)
            result[i] = field.Zero();
    
        for (int row = 0; row < numerrorstocorrect; row++)
        {
            int col = 0;
            // Поиск ведущего элемента в строке
            while (col < numerrorstocorrect && 
                   field.Equals(matrix.Get(row, col), field.Zero()))
                col++;
        
            // Проверка на несовместность системы
            if (col == numerrorstocorrect)
            {
                // Все коэффициенты нулевые, но последний столбец ненулевой
                if (!field.Equals(matrix.Get(row, numerrorstocorrect), field.Zero()))
                    return null; // Система несовместна
            }
            // Запись коэффициента полинома
            else
            {
                result[numerrorstocorrect - col] = matrix.Get(row, numerrorstocorrect);
            }
        }
    
        return result;
    }

    private List<int> FindErrorLocations(T[] errlocpoly, int maxsolutions)
    {
        List<int> errorLocations = new List<int>();
        T genrec = field.Reciprocal(generator);
        T genrecpow = field.One();
        
        for (int i = 0; i < codewordLen; i++)
        {
            T polyval = EvaluatePolynomial(errlocpoly, genrecpow);
            if (field.Equals(polyval, field.Zero()))
            {
                if (errorLocations.Count >= maxsolutions)
                    return null;
                
                errorLocations.Add(i);
            }
            genrecpow = field.Multiply(genrec, genrecpow);
        }
        
        return errorLocations;
    }

    private T[] CalculateErrorValues(List<int> errlocs, T[] syndromes)
    {
        int n = errlocs.Count;
        Matrix<T> matrix = new Matrix<T>(eccLen, n + 1, field);
        
        // Fill the matrix
        for (int c = 0; c < n; c++)
        {
            T genpow = Pow(generator, errlocs[c]);
            T genpowpow = field.One();
            
            for (int r = 0; r < eccLen; r++)
            {
                matrix.Set(r, c, genpowpow);
                genpowpow = field.Multiply(genpow, genpowpow);
            }
        }
        
        // Last column: syndromes
        for (int r = 0; r < eccLen; r++)
            matrix.Set(r, n, syndromes[r]);
        
        matrix.ReducedRowEchelonForm();
        
        // Check consistency
        T[] errvals = new T[n];
        for (int i = 0; i < n; i++)
        {
            if (!field.Equals(matrix.Get(i, i), field.One()))
                return null;
            
            errvals[i] = matrix.Get(i, n);
        }
        
        return errvals;
    }

    private T[] FixErrors(T[] codeword, List<int> errlocs, T[] errvals)
    {
        T[] corrected = (T[])codeword.Clone();
        for (int i = 0; i < errlocs.Count; i++)
        {
            int loc = errlocs[i];
            corrected[loc] = field.Subtract(corrected[loc], errvals[i]);
        }
        return corrected;
    }

    private T EvaluatePolynomial(T[] polynomial, T point)
    {
        T result = field.Zero();
        for (int i = polynomial.Length - 1; i >= 0; i--)
        {
            result = field.Multiply(point, result);
            result = field.Add(polynomial[i], result);
        }
        return result;
    }

    private T Pow(T baseVal, int exp)
    {
        if (exp < 0)
            throw new ArgumentException("Unsupported negative exponent");
        
        T result = field.One();
        for (int i = 0; i < exp; i++)
            result = field.Multiply(baseVal, result);
        
        return result;
    }
}