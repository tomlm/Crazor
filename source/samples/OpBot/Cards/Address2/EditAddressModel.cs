namespace OpBot.Cards.Address2
{
    public class EditAddressModel
    {
        public bool IsEdit { get; set; }

        public Address? Address { get; set; } = new Address();
    }
}
