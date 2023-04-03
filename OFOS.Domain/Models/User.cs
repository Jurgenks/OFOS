using System.ComponentModel.DataAnnotations.Schema;

namespace OFOS.Domain.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string? Password { get; set; }
        [NotMapped]
        public List<Guid> OrderHistory { get; set; }

        public string? AuthenticationToken { get; set; }
        public string? RetrievalToken { get; set; }

        public User(string firstName, string lastName, string email, string? phoneNumber, string address, string city, string region, string postalCode, string country, string? password)
        {
            Id = Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            City = city;
            Region = region;
            PostalCode = postalCode;
            Country = country;
            Password = password;
            OrderHistory = new List<Guid>();

        }
    }
}
