using acaigalatico.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace acaigalatico.Web.Controllers
{
    [Authorize] // Restrict to logged-in users
    public class AdminController : Controller
    {
        private readonly IProdutoService _produtoService;

        public AdminController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _produtoService.GetProdutosAsync();
            return View(products);
        }
    }
}
