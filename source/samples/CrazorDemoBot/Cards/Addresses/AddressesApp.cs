using Crazor;
using Crazor.Mvc;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.Addresses
{
    public class AddressesApp : CardApp
    {
        public AddressesApp(CardAppContext context) : base(context)
        {
        }

        [TimeMemory("yyyyMMdd")]
        public List<Address> Addresses { get; set; } = new List<Address>();

        public Address? GetAddress(string addressId)
        {
            return Addresses.SingleOrDefault(a => a.Id == addressId);
        }

        public bool AddAddress(Address address)
        {
            address.Id = Utils.GetNewId();
            Addresses.Add(address);
            return true;
        }

        public bool UpdateAddress(Address newAddress)
        {
            var address = Addresses.Where(a => a.Id == newAddress.Id).SingleOrDefault();
            if (address != null)
            {
                address.Street = newAddress.Street;
                address.City = newAddress.City;
                address.State = newAddress.State;
                address.PostalCode = newAddress.PostalCode;
                address.Country = newAddress.Country;
                return true;
            }
            return false;
        }

        public bool DeleteAddress(string addressId)
        {
            Addresses = Addresses.Where(a => a.Id != addressId).ToList();
            return true;
        }
    }
}
