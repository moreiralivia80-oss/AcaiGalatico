using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace acaigalatico.Web.Controllers
{
    public class ToppingOption
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    // [Authorize] removed to allow guests to order
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            // For now, we will just render the view.
            // In a real scenario, we would load available fruits and toppings from DB here.
            // Using ViewBag for simulation to match the "prototype" request speed.
            
            ViewBag.Fruits = new List<string> { "Banana", "Morango", "Manga", "Kiwi", "Abacaxi", "Uva" };
            
            var toppings = new List<ToppingOption> { 
                new ToppingOption { Name = "Creme de Paçoca", Description = "Uma camada deliciosa de creme de amendoim.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Creme de Pitaya", Description = "Sabor exótico e cor vibrante.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Creme de Leite em Pó", Description = "Cremosidade extra para seu açaí.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Leite em Pó", Description = "Clássico indispensável.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Granola", Description = "Crocância saudável.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Paçoca", Description = "O sabor do amendoim.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Flocos de Arroz", Description = "Leve e crocante.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Chocoball", Description = "Bolinhas de chocolate crocantes.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Confete", Description = "Colorido e divertido.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Coco Ralado", Description = "Toque tropical.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Aveia", Description = "Opção nutritiva.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Amendoim", Description = "Torrado e sem pele.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Sucrilhos", Description = "Crocante de milho.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Jujuba", Description = "Doce e macia.", ImageUrl = "/images/logo-sem-escrita.png" },
                new ToppingOption { Name = "Gotas de Chocolate", Description = "Pequenas delícias de cacau.", ImageUrl = "/images/logo-sem-escrita.png" }
            };
            
            ViewBag.Toppings = toppings;

            return View();
        }

        [HttpPost]
        public IActionResult SubmitOrder(string type, string size, int quantity, List<string> fruits, List<string> toppings)
        {
            // Logic to process order would go here (save to DB, etc.)
            // For now, redirect to a Success page or back to Home with a message.
            TempData["SuccessMessage"] = "Pedido realizado com sucesso! Em breve entregaremos sua galáxia de sabor.";
            return RedirectToAction("Index", "Home");
        }
    }
}
