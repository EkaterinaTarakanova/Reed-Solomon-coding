namespace ReedSolomon;

public class Matrix<T>
{
    private readonly IField<T> field;
    private readonly T[][] values;
    public int RowCount => values.Length;
    public int ColumnCount => values[0].Length;

    public Matrix(int rows, int cols, IField<T> field)
    {
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Invalid number of rows or columns");
        this.field = field;
        values = new T[rows][];
        for (int i = 0; i < rows; i++)
            values[i] = new T[cols];
    }

    public T Get(int row, int col) => values[row][col];
    public void Set(int row, int col, T value) => values[row][col] = value;

    public Matrix<T> Clone()
    {
        var result = new Matrix<T>(RowCount, ColumnCount, field);
        for (int i = 0; i < RowCount; i++)
            Array.Copy(values[i], result.values[i], ColumnCount);
        return result;
    }

    public Matrix<T> Transpose()
    {
        var result = new Matrix<T>(ColumnCount, RowCount, field);
        for (int i = 0; i < RowCount; i++)
            for (int j = 0; j < ColumnCount; j++)
                result.values[j][i] = values[i][j];
        return result;
    }

    public override string ToString()
    {
        return "[" + string.Join(",\n ", 
            values.Select(row => 
                "[" + string.Join(", ", row.Select(x => x.ToString())) + "]")) + "]";
    }

    public void SwapRows(int row0, int row1)
    {
        (values[row0], values[row1]) = (values[row1], values[row0]);
    }

    public void MultiplyRow(int row, T factor)
    {
        for (int j = 0; j < ColumnCount; j++)
            values[row][j] = field.Multiply(values[row][j], factor);
    }

    public void AddRows(int srcRow, int destRow, T factor)
    {
        for (int j = 0; j < ColumnCount; j++)
        {
            T product = field.Multiply(values[srcRow][j], factor);
            values[destRow][j] = field.Add(values[destRow][j], product);
        }
    }

    public Matrix<T> Multiply(Matrix<T> other)
    {
        if (ColumnCount != other.RowCount)
            throw new ArgumentException("Matrix dimensions mismatch");
        
        var result = new Matrix<T>(RowCount, other.ColumnCount, field);
        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < other.ColumnCount; j++)
            {
                T sum = field.Zero();
                for (int k = 0; k < ColumnCount; k++)
                {
                    T prod = field.Multiply(Get(i, k), other.Get(k, j));
                    sum = field.Add(sum, prod);
                }
                result.Set(i, j, sum);
            }
        }
        return result;
    }

    public void ReducedRowEchelonForm()
    {
        int numPivots = 0;
        for (int j = 0; j < ColumnCount && numPivots < RowCount; j++)
        {
            int pivotRow = numPivots;
            while (pivotRow < RowCount && field.Equals(Get(pivotRow, j), field.Zero()))
                pivotRow++;
            
            if (pivotRow == RowCount) continue;
            
            SwapRows(numPivots, pivotRow);
            pivotRow = numPivots;
            numPivots++;
            
            T pivotValue = Get(pivotRow, j);
            MultiplyRow(pivotRow, field.Reciprocal(pivotValue));
            
            for (int i = pivotRow + 1; i < RowCount; i++)
            {
                T factor = field.Negate(Get(i, j));
                AddRows(pivotRow, i, factor);
            }
        }
        
        for (int i = numPivots - 1; i >= 0; i--)
        {
            int pivotCol = 0;
            while (pivotCol < ColumnCount && field.Equals(Get(i, pivotCol), field.Zero()))
                pivotCol++;
            
            if (pivotCol == ColumnCount) continue;
            
            for (int k = 0; k < i; k++)
            {
                T factor = field.Negate(Get(k, pivotCol));
                AddRows(i, k, factor);
            }
        }
    }

    public void Invert()
    {
        if (RowCount != ColumnCount)
            throw new InvalidOperationException("Matrix must be square");
        
        var temp = new Matrix<T>(RowCount, ColumnCount * 2, field);
        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                temp.Set(i, j, Get(i, j));
                temp.Set(i, j + ColumnCount, i == j ? field.One() : field.Zero());
            }
        }
        
        temp.ReducedRowEchelonForm();
        
        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                T expected = i == j ? field.One() : field.Zero();
                if (!field.Equals(temp.Get(i, j), expected))
                    throw new InvalidOperationException("Matrix is not invertible");
                Set(i, j, temp.Get(i, j + ColumnCount));
            }
        }
    }

    public T DeterminantAndREF()
    {
        if (RowCount != ColumnCount)
            throw new InvalidOperationException("Matrix must be square");
        
        var clone = Clone();
        T det = field.One();
        int numPivots = 0;
        
        for (int j = 0; j < clone.ColumnCount; j++)
        {
            int pivotRow = numPivots;
            while (pivotRow < clone.RowCount && 
                   field.Equals(clone.Get(pivotRow, j), field.Zero()))
                pivotRow++;
            
            if (pivotRow == clone.RowCount) continue;
            
            if (pivotRow != numPivots)
            {
                clone.SwapRows(numPivots, pivotRow);
                det = field.Negate(det);
            }
            
            T pivotVal = clone.Get(numPivots, j);
            det = field.Multiply(det, pivotVal);
            clone.MultiplyRow(numPivots, field.Reciprocal(pivotVal));
            
            for (int i = numPivots + 1; i < clone.RowCount; i++)
            {
                T factor = field.Negate(clone.Get(i, j));
                clone.AddRows(numPivots, i, factor);
            }
            numPivots++;
        }
        return det;
    }
}