using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class OptionTemplate
    {
        public int OptionTemplateID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Slug { get; set; }

        [Required, MaxLength(20)]
        public string ValueType { get; set; } = "freetext";
        // Values: "predefined" / "numeric" / "range" / "freetext"

        public string? PredefinedValues { get; set; } // Comma-separated e.g. "Black,Red,Blue"

        public bool AllowCustomValue { get; set; } = true;

        [MaxLength(20)]
        public string? Suffix { get; set; }

        [MaxLength(500)]
        public string? HelpText { get; set; }

        [MaxLength(100)]
        public string? Placeholder { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
