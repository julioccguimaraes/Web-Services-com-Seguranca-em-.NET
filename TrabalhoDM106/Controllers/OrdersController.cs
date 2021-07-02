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