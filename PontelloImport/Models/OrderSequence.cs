namespace PontelloImport.Models
{
    public class OrderSequence
    {
        public int Id { get; set; }          // Always 1 — single row table
        public int LastUsedNumber { get; set; } = 0;
    }
}
