namespace OpBot.Cards.Address
{
    public class EditAddressModel
    {
        public bool IsEdit { get; set; }

        public Address? Address { get; set; } = new Address();
    }
}
