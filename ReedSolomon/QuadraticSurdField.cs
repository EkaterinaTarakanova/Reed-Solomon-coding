namespace ReedSolomon;

public class QuadraticSurdField : IField<QuadraticSurd>
{
    private readonly int d;
    public int D => d;

    public QuadraticSurdField(int d) => this.d = d;

    private void Check(QuadraticSurd x)
    {
        if (x.D != d)
            throw new ArgumentException("The value under the square root must match that of the field");
    }

    public QuadraticSurd Zero() => new QuadraticSurd(0, 0, 1, d);
    public QuadraticSurd One() => new QuadraticSurd(1, 0, 1, d);
    public bool Equals(QuadraticSurd x, QuadraticSurd y)
    {
        Check(x); Check(y);
        return x.Equals(y);
    }
    public QuadraticSurd Negate(QuadraticSurd x)
    {
        Check(x);
        return new QuadraticSurd(-x.A, -x.B, x.C, d);
    }
    public QuadraticSurd Add(QuadraticSurd x, QuadraticSurd y)
    {
        Check(x); Check(y);
        return new QuadraticSurd(
            x.A * y.C + y.A * x.C,
            x.B * y.C + y.B * x.C,
            x.C * y.C,
            d
        );
    }
    public QuadraticSurd Subtract(QuadraticSurd x, QuadraticSurd y) => Add(x, Negate(y));
    public QuadraticSurd Reciprocal(QuadraticSurd x)
    {
        Check(x);
        return new QuadraticSurd(
            -x.A * x.C,
            x.B * x.C,
            x.B * x.B * d - x.A * x.A,
            d
        );
    }
    public QuadraticSurd Multiply(QuadraticSurd x, QuadraticSurd y)
    {
        Check(x); Check(y);
        return new QuadraticSurd(
            x.A * y.A + x.B * y.B * d,
            x.A * y.B + y.A * x.B,
            x.C * y.C,
            d
        );
    }
    public QuadraticSurd Divide(QuadraticSurd x, QuadraticSurd y) => Multiply(x, Reciprocal(y));
}