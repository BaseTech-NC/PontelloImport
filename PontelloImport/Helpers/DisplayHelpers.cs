namespace PontelloImport.Helpers
{
    public static class DisplayHelpers
    {
        public static string VariantDisplay(string? variantTitle)
        {
            if (string.IsNullOrWhiteSpace(variantTitle) ||
                variantTitle.Trim().ToLower() == "default title" ||
                variantTitle.Trim().ToLower() == "title")
                return "N/A";
            return variantTitle;
        }
    }
}
