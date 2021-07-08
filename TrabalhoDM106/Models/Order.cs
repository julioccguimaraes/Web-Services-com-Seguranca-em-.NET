using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoDM106.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderProduct = new HashSet<OrderProduct>();
        }

        public int Id { get; set; }

        public string Email { get; set; }

        public DateTime Data { get; set; }

        public DateTime? DataEntrega { get; set; }

        public string Status { get; set; }

        public decimal Total { get; set; }

        public float Peso { get; set; }

        public decimal Frete { get; set; }

        public virtual ICollection<OrderProduct> OrderProduct { get; set; }
    }
}