using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class Product
    {
        [Key]
        public int ID { get; set; }
        public string? Handle { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public int InventoryQuantity { get; set; }
        public string? Type { get; set; }


    }
}
