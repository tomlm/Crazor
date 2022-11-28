namespace Crazor.Attributes
{
    /// <summary>
    /// This memory is keyed off of a time and a pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TimeMemoryAttribute : MemoryAttribute
    {
        public TimeMemoryAttribute(string pattern)
        {
            this.Pattern = pattern;
        }

        /// <summary>
        /// Pattern for time key, like yyyyMMdd
        /// </summary>
        public string Pattern { get; set; }

        public override string? GetKey(object obj)
        {
            CardApp cardApp = obj as CardApp;
            if (cardApp != null)
            {
                var rootKey = cardApp.Activity?.LocalTimestamp?.ToString(Pattern);
                if (rootKey != null)
                {
                    return $"TM-{rootKey}";
                }
            }
            return null;
        }
    }
}
