namespace PontelloImport.Models
	{
	public interface IAuditable
		{
		int? CreatedBy { get; set; }
		DateTime? CreatedDate { get; set; }
		int? ModifiedBy { get; set; }
		DateTime? ModifiedDate { get; set; }
		}
	}