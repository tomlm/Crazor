namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConversationMemoryAttribute : MemoryAttribute
    {
        public ConversationMemoryAttribute()
        {
        }

        public override string? GetKey(object obj)
        {
            var cardApp = obj as CardApp;
            return cardApp?.Activity?.From?.Id;
        }
    }
}
