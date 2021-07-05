using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoDM106.CRMClient
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(12)]
        public string Cpf { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Zip { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Mobile { get; set; }
    }
}