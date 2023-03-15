namespace OFOS.Domain.Models
{
    public class Restaurant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public List<Product> Products { get; set; }
        public Contact Owner { get; set; }

        public Restaurant(string name, string description, string address, string city, string region, string postalCode, string country, List<Product> products, Contact owner)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Address = address;
            City = city;
            Region = region;
            PostalCode = postalCode;
            Country = country;
            Products = products;
            Owner = owner;
        }
    }
}
