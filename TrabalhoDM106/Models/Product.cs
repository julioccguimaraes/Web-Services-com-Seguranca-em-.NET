using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoDM106.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string nome { get; set; }

        public string descricao { get; set; }

        [Required]
        public string codigo { get; set; }

        [Required]
        public string modelo { get; set; }

        public decimal preco { get; set; }

        public string cor { get; set; }
        
        public float peso { get; set; }

        public float altura { get; set; }

        public float largura { get; set; }

        public float comprimento { get; set; }

        public float diametro { get; set; }
    }
}