using acaigalatico.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace acaigalatico.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Apenas usuários com a role Admin
    public class AdminController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly acaigalatico.Infrastructure.Context.AppDbContext _context;

        public AdminController(IProdutoService produtoService, acaigalatico.Infrastructure.Context.AppDbContext context)
        {
            _produtoService = produtoService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try 
            {
                // 1. Produtos
                var products = await _produtoService.GetProdutosAsync() ?? new List<acaigalatico.Application.DTOs.ProdutoDTO>();
                
                // 2. Clientes (Limitado para evitar sobrecarga)
                var clients = _context.Clientes.Take(100).ToList() ?? new List<Domain.Entities.Cliente>();
                
                // 3. Vendas Hoje (Cálculo robusto sem usar .Date na query)
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var totalVendasHoje = _context.Vendas
                    .Count(v => v.DataVenda >= today && v.DataVenda < tomorrow);
                
                // 4. Últimas Vendas
                var sales = _context.Vendas
                    .OrderByDescending(v => v.DataVenda)
                    .Take(20)
                    .ToList() ?? new List<Domain.Entities.Venda>();
                
                // 5. Faturamento Total
                var faturamentoTotal = _context.Vendas
                    .Sum(v => (decimal?)v.ValorTotal) ?? 0;

                ViewBag.VendasHoje = totalVendasHoje;
                ViewBag.TotalClientes = _context.Clientes.Count();
                ViewBag.Faturamento = faturamentoTotal;
                ViewBag.Clients = clients;
                ViewBag.Sales = sales;

                return View(products);
            }
            catch (Exception ex)
            {
                // Tenta carregar pelo menos os produtos se o resto falhar
                try {
                    var products = await _produtoService.GetProdutosAsync();
                    ViewBag.VendasHoje = 0;
                    ViewBag.TotalClientes = 0;
                    ViewBag.Faturamento = 0m;
                    ViewBag.Clients = new List<Domain.Entities.Cliente>();
                    ViewBag.Sales = new List<Domain.Entities.Venda>();
                    TempData["ErrorMessage"] = "Aviso: Algumas métricas não puderam ser carregadas. | " + ex.Message;
                    return View(products);
                } catch {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        // --- AÇÕES PARA EDITAR DADOS DIRETAMENTE (VIA ADMIN) ---
        
        [HttpGet]
        public IActionResult EditMetrics()
        {
            // Busca os dados atuais para preencher o formulário
            ViewBag.VendasHoje = _context.Vendas.Where(v => v.DataVenda.Date == DateTime.Today).Count();
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.Faturamento = _context.Vendas.Sum(v => (decimal?)v.ValorTotal) ?? 0;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMetrics(int vendasHoje, int totalClientes, string faturamento)
        {
            string etapa = "Início";
            try
            {
                // 0. Parse do faturamento
                etapa = "Parse do faturamento";
                string fatClean = faturamento.Replace("R$", "").Replace(" ", "").Trim();
                if (!decimal.TryParse(fatClean, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out decimal fatDecimal))
                {
                    if (!decimal.TryParse(fatClean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fatDecimal))
                    {
                        throw new Exception("Formato de faturamento inválido.");
                    }
                }

                // 1. Ajuste de Clientes
                etapa = "Ajuste de Clientes";
                int currentClientes = _context.Clientes.Count();
                if (totalClientes > currentClientes)
                {
                    int diff = totalClientes - currentClientes;
                    int toAdd = Math.Min(diff, 10); 
                    for (int i = 0; i < toAdd; i++)
                    {
                        _context.Clientes.Add(new Domain.Entities.Cliente { 
                            Nome = $"Cliente {DateTime.Now:HHmm}_{i}", 
                            Telefone = "000000000",
                            SaldoDevedor = 0
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // 2. Ajuste de Vendas Hoje e Faturamento
                etapa = "Ajuste de Vendas e Faturamento";
                int currentVendasHoje = _context.Vendas.Count(v => v.DataVenda.Date == DateTime.Today);
                decimal currentFaturamento = _context.Vendas.Sum(v => (decimal?)v.ValorTotal) ?? 0;

                if (vendasHoje > currentVendasHoje || fatDecimal > currentFaturamento)
                {
                    decimal valorFaltante = Math.Max(0, fatDecimal - currentFaturamento);
                    int diffVendas = Math.Max(0, vendasHoje - currentVendasHoje);
                    
                    // Se não houver diferença de vendas mas houver de faturamento, cria 1 venda
                    if (diffVendas == 0 && valorFaltante > 0) diffVendas = 1;

                    for (int i = 0; i < diffVendas; i++)
                    {
                        decimal valorVenda = (i == diffVendas - 1) ? valorFaltante : 0;
                        
                        _context.Vendas.Add(new Domain.Entities.Venda { 
                            ValorTotal = valorVenda, 
                            DataVenda = DateTime.Now,
                            Status = Domain.Entities.StatusVenda.Entregue,
                            FormaPagamento = Domain.Entities.TipoPagamento.Dinheiro,
                            EnderecoEntrega = "Balcao", // Sem acento para evitar problemas
                            BairroEntrega = "Centro",
                            Observacao = "Ajuste"
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Métricas atualizadas com sucesso!";
            }
            catch (Exception ex)
            {
                string msg = $"Erro na etapa '{etapa}': {ex.Message}";
                if (ex.InnerException != null) msg += " | Detalhe: " + ex.InnerException.Message;
                
                System.Console.WriteLine("########## ERRO NO BANCO ##########");
                System.Console.WriteLine(msg);
                System.Console.WriteLine("###################################");

                TempData["ErrorMessage"] = msg;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SeedMockData()
        {
            // Método utilitário para criar alguns dados de teste se o banco estiver vazio
            if (!_context.Clientes.Any())
            {
                _context.Clientes.Add(new Domain.Entities.Cliente { Nome = "Cliente Teste", Telefone = "11999999999" });
                await _context.SaveChangesAsync();
            }

            if (!_context.Vendas.Any())
            {
                _context.Vendas.Add(new Domain.Entities.Venda { ValorTotal = 1200.50m, DataVenda = DateTime.Now });
                _context.Vendas.Add(new Domain.Entities.Venda { ValorTotal = 50.00m, DataVenda = DateTime.Today });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
