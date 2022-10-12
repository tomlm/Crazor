using Crazor;

namespace OpBot.Cards.Address
{
    public class Address
    {
        public string? Id { get; set; } = Utils.GetNewId();

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set; }

        public string? Country { get; set; }
    }
}
