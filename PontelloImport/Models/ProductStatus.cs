namespace PontelloImport.Models
{
    public enum ProductStatus
    {
        Draft = 0,
        Published = 1,
        Unlisted = 2,   // Special order — dealers must call Pontello
        Archived = 3
    }
}
