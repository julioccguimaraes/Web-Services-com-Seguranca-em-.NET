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
    public class ProductsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Products
        public IQueryable<Product> GetProducts()
        {
            return db.Products;
        }

        // GET: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest("A idendificação do produto difere do que deseja alterar.");
            }

            // verifica se há outro produto com o mesmo código.

            // Importante usar AsNoTracking() segundo stackoverflow:
            // "I believe you might have invoked Select before the update, By default,
            // DBContext will cache the record when they are fethced (Selected),
            // use "AsNoTracking()" in your select call while fetching record."
            // fonte: stackoverflow.com/questions/41376161/attaching-an-entity-of-type-x-failed-because-another-entity-of-the-same-type-a
            Product productByCode = db.Products.AsNoTracking().First(p => p.codigo == product.codigo);

            if (productByCode != null && productByCode.Id != product.Id)
            {
                return BadRequest("Já existe outro produto com o mesmo código " + productByCode.codigo);
            }

            // verifica se há outro produto com o mesmo modelo
            Product productByModel = db.Products.AsNoTracking().First(p => p.modelo == product.modelo);

            if (productByModel != null && productByModel.Id != product.Id) {
                return BadRequest("Já existe outro produto com o mesmo modelo " + productByModel.modelo);
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // POST: api/Products
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.Id == id) > 0;
        }
    }
}