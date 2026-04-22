using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab3Ano.Models.ViewModels
{
    public class CreateAnuncioViewModel
    {
        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "O ano é obrigatório")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "A quilometragem é obrigatória")]
        public int Quilometragem { get; set; }

        [Required(ErrorMessage = "A Potência é obrigatória")]
        [Range(1, 2000, ErrorMessage = "Valor inválido")]
        [Display(Name = "Potência (CV)")]
        public int Potencia { get; set; }

        [Required(ErrorMessage = "A Cilindrada é obrigatória")]
        [Range(50, 10000, ErrorMessage = "Valor inválido")]
        [Display(Name = "Cilindrada (cm3)")]
        public int Cilindrada { get; set; }

        public string Caixa { get; set; } = "Manual";

        [Display(Name = "Marca")]
        public int IdMarca { get; set; }

        [Display(Name = "Modelo")]
        public int IdModelo { get; set; }

        [Display(Name = "Combustível")]
        public int IdCombustivel { get; set; }

        [Display(Name = "Categoria")]
        public int IdCategoria { get; set; }

        // --- IMAGENS ---
        [Display(Name = "Fotografias")]
        public List<IFormFile>? Fotos { get; set; }
    }
}