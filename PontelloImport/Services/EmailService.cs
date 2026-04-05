using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;

namespace PontelloImport.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string toEmail,
            string toName, string subject, string htmlBody)
        {
            var cfg = _config.GetSection("EmailConfiguration");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                cfg["SmtpFromName"],
                cfg["SmtpUsername"]));
            message.To.Add(new MailboxAddress(
                toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html")
                { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                cfg["SmtpServer"],
                int.Parse(cfg["SmtpPort"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                cfg["SmtpUsername"],
                cfg["SmtpPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendDealerApprovedAsync(
            string toEmail, string firstName,
            string tempPassword)
        {
            var html = $@"
            <div style='font-family:sans-serif;
                 max-width:560px;margin:0 auto;'>
              <div style='background:#0D3D38;padding:24px;
                   border-radius:8px 8px 0 0;'>
                <h2 style='color:#02C39A;margin:0;
                    font-size:20px;'>Pontello Imports</h2>
                <p style='color:rgba(255,255,255,0.7);
                   margin:4px 0 0;font-size:13px;'>
                  Dealer Portal
                </p>
              </div>
              <div style='background:#f8fafc;padding:24px;
                   border:1px solid #e2e8f0;
                   border-top:none;
                   border-radius:0 0 8px 8px;'>
                <h3 style='margin:0 0 12px;color:#111827;'>
                  Welcome, {firstName}!
                </h3>
                <p style='color:#4b5563;font-size:14px;
                   line-height:1.6;'>
                  Your dealer application has been approved.
                  You can now log in to the Pontello Imports
                  dealer portal.
                </p>
                <div style='background:#fff;border:1px solid
                     #e2e8f0;border-radius:6px;padding:16px;
                     margin:16px 0;'>
                  <p style='margin:0 0 8px;font-size:13px;
                     color:#6b7280;'>Your login credentials</p>
                  <p style='margin:0 0 4px;font-size:14px;'>
                    <strong>Email:</strong> {toEmail}
                  </p>
                  <p style='margin:0;font-size:14px;'>
                    <strong>Temporary password:</strong>
                    <span style='font-family:monospace;
                      background:#f1f5f9;padding:2px 6px;
                      border-radius:4px;'>
                      {tempPassword}
                    </span>
                  </p>
                </div>
                <p style='color:#4b5563;font-size:13px;'>
                  Please change your password after
                  your first login.
                </p>
                <a href='https://basetech-pontello.azurewebsites.net/Identity/Account/Login'
                   style='display:inline-block;
                   background:#028090;color:#fff;
                   padding:10px 20px;border-radius:6px;
                   text-decoration:none;font-size:14px;
                   margin-top:8px;'>
                  Sign In to Portal
                </a>
                <p style='color:#9ca3af;font-size:12px;
                   margin-top:20px;'>
                  Questions? Call us at 647-964-6833 or
                  email jesse@pontelloimports.com
                </p>
              </div>
            </div>";

            await SendAsync(toEmail, firstName,
                "Welcome to Pontello Imports — Your Dealer Account",
                html);
        }

        public async Task SendOrderSubmittedAsync(
            string adminEmail, string dealerName,
            string poNumber, string orderSummary)
        {
            var html = $@"
            <div style='font-family:sans-serif;
                 max-width:560px;margin:0 auto;'>
              <div style='background:#0D3D38;padding:24px;
                   border-radius:8px 8px 0 0;'>
                <h2 style='color:#02C39A;margin:0;
                    font-size:20px;'>New Order Received</h2>
              </div>
              <div style='background:#f8fafc;padding:24px;
                   border:1px solid #e2e8f0;
                   border-top:none;
                   border-radius:0 0 8px 8px;'>
                <p style='color:#111827;font-size:15px;
                   font-weight:500;margin:0 0 12px;'>
                  PO #{poNumber} — {dealerName}
                </p>
                <div style='background:#fff;border:1px solid
                     #e2e8f0;border-radius:6px;padding:16px;
                     margin:16px 0;font-size:13px;
                     color:#374151;white-space:pre-line;'>
                  {orderSummary}
                </div>
                <a href='https://basetech-pontello.azurewebsites.net/AdminOrders'
                   style='display:inline-block;
                   background:#028090;color:#fff;
                   padding:10px 20px;border-radius:6px;
                   text-decoration:none;font-size:14px;'>
                  Review Order
                </a>
              </div>
            </div>";

            await SendAsync(adminEmail, "Pontello Imports",
                $"New Order — PO #{poNumber} from {dealerName}",
                html);
        }

        public async Task SendOrderStatusChangedAsync(
            string toEmail, string dealerName,
            string poNumber, string newStatus,
            string? note = null)
        {
            var statusColor = newStatus switch {
                "Confirmed" => "#15803d",
                "Shipped"   => "#1d4ed8",
                "Invoiced"  => "#028090",
                "Cancelled" => "#dc2626",
                _           => "#6b7280"
            };

            var noteHtml = note != null
                ? $"<p style='background:#f1f5f9;padding:12px;border-radius:6px;font-size:13px;color:#374151;margin:12px 0;'>{note}</p>"
                : "";

            var html = $@"
            <div style='font-family:sans-serif;
                 max-width:560px;margin:0 auto;'>
              <div style='background:#0D3D38;padding:24px;
                   border-radius:8px 8px 0 0;'>
                <h2 style='color:#02C39A;margin:0;
                    font-size:20px;'>Order Update</h2>
              </div>
              <div style='background:#f8fafc;padding:24px;
                   border:1px solid #e2e8f0;
                   border-top:none;
                   border-radius:0 0 8px 8px;'>
                <p style='color:#4b5563;font-size:14px;
                   margin:0 0 16px;'>
                  Hi {dealerName}, your order status
                  has been updated.
                </p>
                <div style='display:flex;align-items:center;
                     gap:12px;margin-bottom:16px;'>
                  <span style='font-size:14px;
                    font-weight:500;color:#111827;'>
                    PO #{poNumber}
                  </span>
                  <span style='background:{statusColor};
                    color:#fff;padding:3px 10px;
                    border-radius:12px;font-size:12px;'>
                    {newStatus}
                  </span>
                </div>
                {noteHtml}
                <a href='https://basetech-pontello.azurewebsites.net/Shop/OrderHistory'
                   style='display:inline-block;
                   background:#028090;color:#fff;
                   padding:10px 20px;border-radius:6px;
                   text-decoration:none;font-size:14px;'>
                  View Order History
                </a>
                <p style='color:#9ca3af;font-size:12px;
                   margin-top:20px;'>
                  Questions? 647-964-6833 or
                  jesse@pontelloimports.com
                </p>
              </div>
            </div>";

            await SendAsync(toEmail, dealerName,
                $"Order Update — PO #{poNumber} is now {newStatus}",
                html);
        }

        public async Task SendPurchaseOrderAsync(
            string dealerEmail, string dealerName,
            string adminEmail, string poNumber,
            byte[] pdfBytes)
        {
            var cfg = _config.GetSection("EmailConfiguration");
            var subject = $"Purchase Order #{poNumber} — Pontello Imports";
            var fileName = $"PO-{poNumber}.pdf";

            var dealerHtml = $@"
            <div style='font-family:sans-serif;max-width:560px;margin:0 auto;'>
              <div style='background:#0D3D38;padding:24px;border-radius:8px 8px 0 0;'>
                <h2 style='color:#02C39A;margin:0;font-size:20px;'>Pontello Imports</h2>
              </div>
              <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;
                   border-top:none;border-radius:0 0 8px 8px;'>
                <p style='color:#111827;font-size:15px;margin:0 0 12px;'>
                  Hi {dealerName},
                </p>
                <p style='color:#4b5563;font-size:14px;line-height:1.6;'>
                  Your purchase order <strong>PO #{poNumber}</strong> has been
                  submitted to Pontello Imports and is under review.
                  Please find your purchase order attached.
                </p>
                <p style='color:#9ca3af;font-size:12px;margin-top:20px;'>
                  Questions? Call us at 647-964-6833 or
                  email jesse@pontelloimports.com
                </p>
              </div>
            </div>";

            var adminHtml = $@"
            <div style='font-family:sans-serif;max-width:560px;margin:0 auto;'>
              <div style='background:#0D3D38;padding:24px;border-radius:8px 8px 0 0;'>
                <h2 style='color:#02C39A;margin:0;font-size:20px;'>New Order Received</h2>
              </div>
              <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;
                   border-top:none;border-radius:0 0 8px 8px;'>
                <p style='color:#111827;font-size:15px;font-weight:500;margin:0 0 12px;'>
                  PO #{poNumber} — {dealerName}
                </p>
                <p style='color:#4b5563;font-size:14px;line-height:1.6;'>
                  New order received from {dealerName}.
                  PO #{poNumber} is attached for your records.
                </p>
              </div>
            </div>";

            async Task SendWithAttachment(
                string toEmail, string toName, string htmlBody)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    cfg["SmtpFromName"], cfg["SmtpUsername"]));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var htmlPart = new TextPart("html") { Text = htmlBody };
                var attachment = new MimePart("application", "pdf")
                {
                    Content = new MimeContent(new MemoryStream(pdfBytes)),
                    ContentDisposition = new ContentDisposition(
                        ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = fileName
                };

                var multipart = new Multipart("mixed");
                multipart.Add(htmlPart);
                multipart.Add(attachment);
                message.Body = multipart;

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    cfg["SmtpServer"],
                    int.Parse(cfg["SmtpPort"] ?? "587"),
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    cfg["SmtpUsername"], cfg["SmtpPassword"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            await SendWithAttachment(dealerEmail, dealerName, dealerHtml);
            await SendWithAttachment(adminEmail, "Pontello Imports", adminHtml);
        }
    }
}
