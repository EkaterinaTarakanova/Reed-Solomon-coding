namespace ReedSolomon;

public class QuadraticSurd : IEquatable<QuadraticSurd>
{
    public int A { get; }
    public int B { get; }
    public int C { get; }
    public int D { get; }

    public QuadraticSurd(int a, int b, int c, int d)
    {
        if (c == 0) throw new DivideByZeroException("Division by zero");
        if (c < 0)
        {
            a = -a;
            b = -b;
            c = -c;
        }
        int gcd = GCD(GCD(a, b), c);
        if (gcd != 0)
        {
            a /= gcd;
            b /= gcd;
            c /= gcd;
        }
        A = a;
        B = b;
        C = c;
        D = d;
    }

    private static int GCD(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public bool Equals(QuadraticSurd other)
    {
        if (other == null) return false;
        return A == other.A && B == other.B && C == other.C && D == other.D;
    }

    public override bool Equals(object obj) => Equals(obj as QuadraticSurd);
    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
    public override string ToString() => $"({A} + {B}*sqrt({D})) / {C}";
}