using System.Numerics;

namespace ReedSolomon;

public class BinaryField : IField<int>
{
    private readonly int modulus;
    private readonly int size;
    public int Modulus => modulus;
    public int Size => size;

    public BinaryField(int mod)
    {
        if (mod <= 1) throw new ArgumentException("Invalid modulus");
        modulus = mod;
        size = 1 << (BitLength(mod) - 1);
    }

    private static int BitLength(int n)
    {
        if (n == 0) return 0;
        return 32 - BitOperations.LeadingZeroCount((uint)n);
    }

    private int Check(int x)
    {
        if (x < 0 || x >= size)
            throw new ArgumentException("Not an element of this field: " + x);
        return x;
    }

    public int Zero() => 0;
    public int One() => 1;
    public bool Equals(int x, int y) => Check(x) == Check(y);
    public int Negate(int x) => Check(x);
    public int Add(int x, int y) => Check(x) ^ Check(y);
    public int Subtract(int x, int y) => Add(x, y);
    public int Multiply(int x, int y)
    {
        x = Check(x);
        y = Check(y);
        int result = 0;
        while (y != 0)
        {
            if ((y & 1) != 0)
                result ^= x;
            x <<= 1;
            if (x >= size)
                x ^= modulus;
            y >>= 1;
        }
        return result;
    }
    public int Reciprocal(int w)
    {
        int x = modulus;
        int y = Check(w);
        if (y == 0) throw new DivideByZeroException();
        
        int a = 0, b = 1;
        while (y != 0)
        {
            (int q, int r) = DivideAndRemainder(x, y);
            x = y;
            y = r;
            int temp = a;
            a = b;
            b = temp ^ Multiply(q, b);
        }
        return x == 1 ? a : throw new InvalidOperationException("Field modulus is not irreducible");
    }
    public int Divide(int x, int y) => Multiply(x, Reciprocal(y));

    private (int quotient, int remainder) DivideAndRemainder(int x, int y)
    {
        if (y == 0) throw new DivideByZeroException();
        int quotient = 0;
        int ylen = BitLength(y);
        int xlen = BitLength(x);
        
        for (int i = xlen - ylen; i >= 0; i--)
        {
            if ((x >> (ylen + i)) != 0)
            {
                x ^= y << i;
                quotient |= 1 << i;
            }
        }
        return (quotient, x);
    }
}