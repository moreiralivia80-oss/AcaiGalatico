using acaigalatico.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace acaigalatico.Web.Controllers
{
    // [Authorize] // Comentado temporariamente ate configurar o Identity
    public class ProductsController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly ICategoriaService _categoriaService;

        // 1. Construtor: Aqui o site "pede" o serviço de produtos
        public ProductsController(IProdutoService produtoService, ICategoriaService categoriaService)
        {
            _produtoService = produtoService;
            _categoriaService = categoriaService;
        }

        // 2. Ação Index: Busca os produtos e manda para a tela
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Vai na Application -> Vai na Infra -> Vai no Banco
            var products = await _produtoService.GetProdutosAsync();
            
            // Retorna a View (Tela) entregando a lista de produtos
            return View(products);
        }

        // --- ADICIONE DAQUI PARA BAIXO ---

        // GET: Products/Create
        // Esse método SÓ abre a tela vazia para você preencher
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoriaId = new SelectList(await _categoriaService.GetCategoriasAsync(), "Id", "Nome");
            return View();
        }

        // POST: Products/Create
        // Esse método RECEBE os dados quando você clica em "Salvar"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(acaigalatico.Application.DTOs.ProdutoDTO produtoDto)
        {
            if (ModelState.IsValid)
            {
                await _produtoService.AddAsync(produtoDto);
                return RedirectToAction(nameof(Index)); // Volta para a lista
            }
            ViewBag.CategoriaId = new SelectList(await _categoriaService.GetCategoriasAsync(), "Id", "Nome", produtoDto.CategoriaId);
            return View(produtoDto);
        }

        // --- MÉTODOS DE EDIÇÃO (UPDATE) ---

        // GET: Products/Edit/5
        // Busca o produto no banco e joga na tela
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produtoDto = await _produtoService.GetByIdAsync(id);

            if (produtoDto == null) return NotFound();

            ViewBag.CategoriaId = new SelectList(await _categoriaService.GetCategoriasAsync(), "Id", "Nome", produtoDto.CategoriaId);
            return View(produtoDto);
        }

        // POST: Products/Edit
        // Recebe as alterações e salva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(acaigalatico.Application.DTOs.ProdutoDTO produtoDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _produtoService.UpdateAsync(produtoDto);
                }
                catch (System.Exception)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _categoriaService.GetCategoriasAsync(), "Id", "Nome", produtoDto.CategoriaId);
            return View(produtoDto);
        }

        // --- MÉTODOS DE EXCLUSÃO (DELETE) ---

        // GET: Products/Delete/5
        // Pergunta: "Tem certeza que quer apagar este produto?"
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produtoDto = await _produtoService.GetByIdAsync(id);

            if (produtoDto == null) return NotFound();

            return View(produtoDto);
        }

        // POST: Products/Delete/5
        // Ação: O usuário clicou em "Sim, excluir"
        [HttpPost(), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _produtoService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
