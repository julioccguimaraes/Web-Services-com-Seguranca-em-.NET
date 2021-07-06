using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrabalhoDM106.Models;
using TrabalhoDM106.br.com.correios.ws;
using TrabalhoDM106.CRMClient;

namespace TrabalhoDM106.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Orders
        //public IQueryable<Order> GetOrders()
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            //return db.Orders;
            return db.Orders.Include(order => order.OrderProduct).ToList();
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && User.Identity.Name != order.Email)
            {
                return Unauthorized();
            }

            return Ok(order);
        }

        // GET: api/Orders/ByEmail?email=teste@teste.com
        [HttpGet]
        [Route("byemail")]
        public IHttpActionResult GetOrdersByEmail(string email)
        {
            if (!User.IsInRole("ADMIN") && User.Identity.Name != email)
            {
                return Unauthorized();
            }

            var orders = db.Orders.Where(o => o.Email == email).ToList();

            return Ok<List<Order>>(orders);
        }

        // PUT: api/Orders/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            if (!User.IsInRole("ADMIN") && User.Identity.Name != order.Email)
            {
                return Unauthorized();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: api/Orders/Close?id=5
        [HttpGet]
        [Route("close")]
        public IHttpActionResult CloseOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && User.Identity.Name != order.Email)
            {
                return Unauthorized();
            }

            if (order.Frete == 0) {
                return BadRequest("Calcule o frete antes de fechar o pedido!");
            }

            order.Status = "fechado";

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && User.Identity.Name != order.Email)
            {
                return Unauthorized();
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }
        // GET: api/Orders/Frete?id=5
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("ADMIN") && User.Identity.Name != order.Email)
            {
                return Unauthorized();
            }

            if(order.Status != "novo")
            {
                return BadRequest("O pedido de ID " + id + " não está disponível para cálculo de frete pois seu status atual é: " + order.Status);
            }

            if (order.OrderProduct.Count == 0)
            {
                return BadRequest("O pedido de ID " + id + " não contém produtos para cálculo do frete.");
            }

            // Obtendo o CEP do cliente
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(order.Email);

            if (customer == null)
            {
                return BadRequest("Falha ao consultar o CEP do cliente para cálculo do frete. Cliente não encontrado no CRM.");
            }

            // calculando a cubagem, o peso total e o valor total dos produtos
            double cubagem = 0;
            float pesoTotal = 0;
            decimal totalProdutos = 0;

            foreach (OrderProduct p in order.OrderProduct)
            {
                cubagem += p.Product.altura * p.Product.comprimento * p.Product.largura * p.Quantidade;
                pesoTotal += p.Product.peso * p.Quantidade;
                totalProdutos += p.Product.preco * p.Quantidade;
            }

            // de posse da cubagem, obtemos as dimensões do volume calculando a raiz cúbica da cubagem.
            double lados = Math.Pow(cubagem, 1.0 / 3.0);

            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();

            // 04014 é Sedex, 04510 é Pac
            // Para formato 1 (caixa e pacote), não precisamos colocar o diâmtro do produto, deixando o valor 0
            cResultado resultado = correios.CalcPrecoPrazo("", "", "04014", "04713001", customer.Zip, pesoTotal.ToString(), 1, (decimal)lados, (decimal)lados, (decimal)lados, 0, "N", totalProdutos, "N");

            if (!resultado.Servicos[0].Erro.Equals("0"))
            {
                return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + " - " + resultado.Servicos[0].MsgErro);
            }

            decimal valorFrete = Convert.ToDecimal(resultado.Servicos[0].Valor.Replace(",", "."));

            order.Peso = pesoTotal;
            order.Frete = valorFrete;
            order.Total = totalProdutos + valorFrete;
            order.DataEntrega = DateTime.Today.AddDays(Convert.ToDouble(resultado.Servicos[0].PrazoEntrega));

            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}