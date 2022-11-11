namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SharedMemoryAttribute : PropertyValueMemoryAttribute
    {
        public SharedMemoryAttribute() : base("SharedId")
        {
        }
    }
}
