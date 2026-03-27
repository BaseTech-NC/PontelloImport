using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class ProductSpecification
    {
        public int SpecificationID { get; set; }

        public int ProductID { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(500)]
        public string Value { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }
}
