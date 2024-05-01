
namespace Crazor.Attributes
{
    /// <summary>
    /// This memory is keyed off of the Activity.From.Id
    /// </summary>
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
