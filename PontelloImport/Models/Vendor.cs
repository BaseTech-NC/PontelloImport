using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public class Vendor : Auditable
		{
		// ===== PRIMARY KEY =====
		[Key]
		public int VendorID { get; set; }

		// ===== REQUIRED FIELDS =====

		[Required(ErrorMessage = "Vendor name is required")]
		[StringLength(100, ErrorMessage = "Vendor name cannot exceed 100 characters")]
		[Display(Name = "Vendor Name")]
		public string VendorName { get; set; } = "";

		// ===== AUTO-GENERATED FIELDS =====

		[StringLength(100)]
		[Display(Name = "Slug")]
		public string VendorSlug { get; set; } = "";  // Auto-generated from VendorName

		// ===== OPTIONAL FIELDS =====

		[StringLength(100)]
		[Display(Name = "Contact Name")]
		public string? ContactName { get; set; }

		[StringLength(100)]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		[Display(Name = "Contact Email")]
		public string? ContactEmail { get; set; }

		[StringLength(20)]
		[Display(Name = "Contact Phone")]
		[DataType(DataType.PhoneNumber)]
		public string? ContactPhone { get; set; }

		[StringLength(255)]
		[Display(Name = "Website")]
		[DataType(DataType.Url)]
		public string? Website { get; set; }

		[StringLength(50)]
		public string? Country { get; set; }

		[Display(Name = "Notes")]
		public string? Notes { get; set; }

		// ===== STATUS =====

		[Display(Name = "Active")]
		public bool IsActive { get; set; } = true;

		// ===== NAVIGATION PROPERTIES =====
		// Uncomment after Product model is updated in Chunk 4
		public ICollection<Product> Products { get; set; } = new HashSet<Product>();
		}
	}
