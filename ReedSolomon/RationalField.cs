using System.Numerics;

namespace ReedSolomon;

public class RationalField : IField<BigInteger>
{
    public static readonly RationalField FIELD = new RationalField();
    
    public BigInteger Zero() => BigInteger.Zero;
    public BigInteger One() => BigInteger.One;
    public bool Equals(BigInteger x, BigInteger y) => x == y;
    public BigInteger Negate(BigInteger x) => -x;
    public BigInteger Add(BigInteger x, BigInteger y) => x + y;
    public BigInteger Subtract(BigInteger x, BigInteger y) => x - y;
    public BigInteger Reciprocal(BigInteger x) => x == BigInteger.One ? x : throw new DivideByZeroException();
    public BigInteger Multiply(BigInteger x, BigInteger y) => x * y;
    public BigInteger Divide(BigInteger x, BigInteger y) => x / y;
}