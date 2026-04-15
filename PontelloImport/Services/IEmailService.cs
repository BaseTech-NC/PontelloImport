namespace PontelloImport.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName,
            string subject, string htmlBody);

        Task SendDealerApprovedAsync(string toEmail,
            string firstName, string tempPassword);

        Task SendOrderSubmittedAsync(string adminEmail,
            string dealerName, string poNumber,
            string orderSummary);

        Task SendOrderStatusChangedAsync(string toEmail,
            string dealerName, string poNumber,
            string newStatus, string? note = null);

        Task SendPurchaseOrderAsync(
            string dealerEmail, string dealerName,
            string adminEmail, string poNumber,
            byte[] pdfBytes,
            bool isRevised = false);
    }
}
