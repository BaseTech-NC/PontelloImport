using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public abstract class Auditable : IAuditable
		{
		[ScaffoldColumn(false)]
		[Display(Name = "Created By")]
		public int? CreatedBy { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Created Date")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
		public DateTime? CreatedDate { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Modified By")]
		public int? ModifiedBy { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Modified Date")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
		public DateTime? ModifiedDate { get; set; }
		}
	}
