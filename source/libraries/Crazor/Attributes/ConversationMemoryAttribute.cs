namespace Crazor.Attributes
{
    /// <summary>
    /// This property will be persisted scoped to the activity.Conversation.Id
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConversationMemoryAttribute : MemoryAttribute
    {
        public ConversationMemoryAttribute()
        {
        }

        public override string? GetKey(object obj)
        {
            var cardApp = obj as CardApp;
            return cardApp?.Activity?.Conversation?.Id;
        }
    }
}
