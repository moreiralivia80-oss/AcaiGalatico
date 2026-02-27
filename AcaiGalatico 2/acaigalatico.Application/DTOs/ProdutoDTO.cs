using System.ComponentModel.DataAnnotations;

namespace acaigalatico.Application.DTOs
{
    public class ProdutoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O preço de venda é obrigatório.")]
        public decimal PrecoVenda { get; set; }

        public int Estoque { get; set; }

        [StringLength(255, ErrorMessage = "A URL da imagem deve ter no máximo 255 caracteres.")]
        public string? ImagemUrl { get; set; }

        [Required(ErrorMessage = "O ID da categoria é obrigatório.")]
        public int CategoriaId { get; set; }

        public string? NomeCategoria { get; set; }
    }
}
