using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class Address
    {
        public int AddressID { get; set; }

        [Required, MaxLength(255)]
        public string Street { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; }

        [Required, MaxLength(50)]
        public string Province { get; set; }

        [Required, MaxLength(10)]
        public string PostalCode { get; set; }

        [Required, MaxLength(50)]
        public string Country { get; set; } = "Canada";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
