using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acaigalatico.Web.Models;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace acaigalatico.Web.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SendMessage(string nome, string email, string telefone, string assunto, string mensagem)
    {
        string successMessage = "Mensagem enviada com sucesso!";

        // 1. Tentar Envio via SMTP (se configurado)
        try 
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"]; // PRECISA PREENCHER NO APPSETTINGS.JSON
            var smtpServer = emailSettings["SmtpServer"];
            var portStr = emailSettings["Port"];
            var enableSslStr = emailSettings["EnableSsl"];

            if (!string.IsNullOrEmpty(senderPassword) && !string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(portStr) && !string.IsNullOrEmpty(enableSslStr) && !string.IsNullOrEmpty(senderEmail))
            {
                using (var client = new SmtpClient(smtpServer, int.Parse(portStr)))
                {
                    client.EnableSsl = bool.Parse(enableSslStr);
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "Açaí Galáctico Site"),
                        Subject = $"[Site] {assunto} - {nome}",
                        Body = $"Nome: {nome}\nEmail: {email}\nTelefone: {telefone}\n\nMensagem:\n{mensagem}",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add("liuliuvks@gmail.com"); // Destinatário fixo

                    client.Send(mailMessage);
                }
            }
            else
            {
                // Se não tiver senha, salva em arquivo para não perder a mensagem
                SaveMessageToFile(nome, email, telefone, assunto, mensagem);
                successMessage = "Mensagem salva no sistema! (Para envio real, configure a senha no appsettings.json)";
            }
        }
        catch (Exception ex)
        {
            // Se der erro no envio, salva em arquivo também
            SaveMessageToFile(nome, email, telefone, assunto, mensagem);
            Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            // Mostra o erro real para o usuário para ajudar no debug
            successMessage = $"Erro SMTP: {ex.Message}. Mensagem salva em 'Emails_Enviados'.";
        }

        TempData["SuccessMessage"] = successMessage;
        return RedirectToAction("Contact");
    }

    private void SaveMessageToFile(string nome, string email, string telefone, string assunto, string mensagem)
    {
        try
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AcaiGalatico 2", "Emails_Enviados");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"Msg_{DateTime.Now:yyyyMMdd_HHmmss}_{nome.Replace(" ", "_")}.txt";
            string filePath = Path.Combine(folderPath, fileName);

            string content = $"DATA: {DateTime.Now}\n" +
                             $"DE: {nome} ({email})\n" +
                             $"TELEFONE: {telefone}\n" +
                             $"ASSUNTO: {assunto}\n" +
                             $"--------------------------------------------------\n" +
                             $"{mensagem}\n" +
                             $"--------------------------------------------------\n";

            System.IO.File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar arquivo: {ex.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
