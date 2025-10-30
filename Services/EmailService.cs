using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
	private readonly string _from = "joaocarlos8221@gmail.com";
	private readonly string _password = "pubq ionq nimd dwnl";

	public async Task SendEmailAsync(string to, string subject, string body)
	{
		using (var client = new SmtpClient("smtp.gmail.com", 587))
		{
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential(_from, _password);

			var mail = new MailMessage();
			mail.From = new MailAddress(_from, "Sistema JwtAuth");
			mail.To.Add(to);
			mail.Subject = subject;
			mail.Body = body;
			mail.IsBodyHtml = true;

			await client.SendMailAsync(mail);
		}
	}
}
