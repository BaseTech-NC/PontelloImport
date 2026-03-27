namespace PontelloImport.Models
	{
	/// <summary>
	/// Represents the publication status of a product or variant
	/// </summary>
	public enum ProductStatus
		{
		/// <summary>
		/// Product created but never published (work in progress)
		/// </summary>
		Draft = 0,

		/// <summary>
		/// Product currently visible to dealers
		/// </summary>
		Published = 1,

		/// <summary>
		/// Product was published but has been removed from sale
		/// </summary>
		Archived = 2
		}
	}