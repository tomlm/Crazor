namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UserMemoryAttribute : MemoryAttribute
    {
        public UserMemoryAttribute()
        {
        }

        public override string? GetKey(object obj)
        {
            var cardApp = obj as CardApp;
            return cardApp?.Activity?.From?.Id;
        }
    }
}
