namespace Crazor
{
    public class CardResult
    {
        /// <summary>
        /// Name of the screen that produced the result
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// true if successful, false if error or canceled
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Result object
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// Message if canceled or failed.
        /// </summary>
        public string? Message { get; set; }

        public T? AsResult<T>()
        {
            ArgumentNullException.ThrowIfNull(Result);
            return ObjectPath.MapValueTo<T>(Result);
        }
    }
}
