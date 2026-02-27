using System.Collections.Generic;
using System.Threading.Tasks;
using acaigalatico.Domain.Entities;
using acaigalatico.Domain.Interfaces;
using acaigalatico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace acaigalatico.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<Categoria> GetByIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }
    }
}
