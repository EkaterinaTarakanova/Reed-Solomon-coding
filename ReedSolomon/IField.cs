namespace ReedSolomon;

public interface IField<T>
{
    T Zero();
    T One();
    bool Equals(T x, T y);
    T Negate(T x);
    T Add(T x, T y);
    T Subtract(T x, T y);
    T Reciprocal(T x);
    T Multiply(T x, T y);
    T Divide(T x, T y);
}