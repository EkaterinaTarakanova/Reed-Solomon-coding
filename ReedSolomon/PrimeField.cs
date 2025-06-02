using System.Numerics;

namespace ReedSolomon;

public class PrimeField : IField<int>
{
    private readonly int modulus;
    public int Modulus => modulus;

    public PrimeField(int mod)
    {
        if (mod < 2)
            throw new ArgumentException("Modulus must be prime");
        modulus = mod;
    }

    private int Check(int x)
    {
        if (x < 0 || x >= modulus)
            throw new ArgumentException("Not an element of this field: " + x);
        return x;
    }

    public int Zero() => 0;
    public int One() => 1;
    public bool Equals(int x, int y) => Check(x) == Check(y);
    public int Negate(int x) => (modulus - Check(x)) % modulus;
    public int Add(int x, int y) => (Check(x) + Check(y)) % modulus;
    public int Subtract(int x, int y) => (Check(x) - Check(y) + modulus) % modulus;
    public int Reciprocal(int w)
    {
        int x = Check(w);
        if (x == 0) throw new DivideByZeroException();
        return (int)BigInteger.ModPow(x, modulus - 2, modulus);
    }
    public int Multiply(int x, int y) => (Check(x) * Check(y)) % modulus;
    public int Divide(int x, int y) => Multiply(x, Reciprocal(y));
}