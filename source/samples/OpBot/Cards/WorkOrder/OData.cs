namespace OpBot.Cards.WorkOrder
{
    public class OData<T> : HasExtensionData
    {
        public T? Value { get; set; }
    }
}
