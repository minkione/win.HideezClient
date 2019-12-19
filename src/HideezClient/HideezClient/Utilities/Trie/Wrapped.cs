namespace HideezClient.Utilities
{
    /// <summary>
    /// Wrapped type.
    /// </summary>
    /// <remarks>Useful to avoid pass by reference parameters.</remarks>
    internal class Wrapped<T>
    {
        public T Value { get; set; }

        public Wrapped(T value)
        {
            Value = value;
        }
    }
}
