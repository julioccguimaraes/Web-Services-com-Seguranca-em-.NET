using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrabalhoDM106.Models
{
    public class OrderProduct
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantidade { get; set; }

        public virtual Product Product { get; set; }

    }
}