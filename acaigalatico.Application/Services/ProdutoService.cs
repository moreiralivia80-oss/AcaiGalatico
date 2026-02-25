using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using acaigalatico.Application.DTOs;
using acaigalatico.Application.Interfaces;
using acaigalatico.Domain.Entities;
using acaigalatico.Domain.Interfaces;

namespace acaigalatico.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public async Task<IEnumerable<ProdutoDTO>> GetProdutosAsync()
        {
            var produtos = await _produtoRepository.GetProdutosAsync();
            
            return produtos.Select(p => new ProdutoDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                PrecoVenda = p.PrecoVenda ?? 0,
                NomeCategoria = p.Categoria?.Nome,
                CategoriaId = p.CategoriaId,
                Estoque = p.EstoqueAtual,
                ImagemUrl = p.ImagemUrl
            });
        }

        public async Task AddAsync(ProdutoDTO produtoDto)
        {
            var produto = new Produto
            {
                Nome = produtoDto.Nome,
                Descricao = produtoDto.Descricao ?? "",
                PrecoVenda = produtoDto.PrecoVenda,
                CategoriaId = produtoDto.CategoriaId,
                EstoqueAtual = produtoDto.Estoque,
                EstoqueMinimo = 5,
                Ativo = true,
                EhParaVenda = true,
                ImagemUrl = string.IsNullOrEmpty(produtoDto.ImagemUrl) ? "/images/produtos/default.png" : produtoDto.ImagemUrl
            };

            await _produtoRepository.CreateAsync(produto);
        }

        public async Task<ProdutoDTO?> GetByIdAsync(int? id)
        {
            if (id == null) return null;
            var p = await _produtoRepository.GetByIdAsync(id.Value);
            if (p == null) return null;
            return new ProdutoDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                PrecoVenda = p.PrecoVenda ?? 0,
                NomeCategoria = p.Categoria?.Nome,
                CategoriaId = p.CategoriaId,
                Estoque = p.EstoqueAtual,
                ImagemUrl = p.ImagemUrl
            };
        }

        public async Task UpdateAsync(ProdutoDTO produtoDto)
        {
            var existing = await _produtoRepository.GetByIdAsync(produtoDto.Id);
            if (existing == null) return;
            
            existing.Nome = produtoDto.Nome;
            existing.Descricao = produtoDto.Descricao ?? "";
            existing.PrecoVenda = produtoDto.PrecoVenda;
            existing.CategoriaId = produtoDto.CategoriaId;
            existing.EstoqueAtual = produtoDto.Estoque;
            existing.ImagemUrl = string.IsNullOrEmpty(produtoDto.ImagemUrl) ? existing.ImagemUrl : produtoDto.ImagemUrl;
            
            await _produtoRepository.UpdateAsync(existing);
        }

        public async Task RemoveAsync(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto != null)
            {
                await _produtoRepository.RemoveAsync(produto);
            }
        }
    }
}
