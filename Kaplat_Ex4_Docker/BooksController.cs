using BookStore.Models;
using BookStore.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BookStore.Controllers
{
    public class BooksController : ApiController
    {
        private static readonly BookStore store = new BookStore();

        [HttpGet]
        [Route("books/health")]
        public HttpResponseMessage Health()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [HttpPost]
        [Route("book")]
        public HttpResponseMessage CreateBook([FromBody] Book newBook)
        {
            try
            {
                Book book = store.AddBook(newBook);
                return Request.CreateResponse(HttpStatusCode.OK, new { result = book.Id });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("books/total")]
        public HttpResponseMessage GetTotalBooks([FromUri] string persistenceMethod)
        {
            try
            {
                int totalBooks = store.GetTotalBooks(persistenceMethod);
                return Request.CreateResponse(HttpStatusCode.OK, new { result = totalBooks });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("book")]
        public HttpResponseMessage GetBookById([FromUri] int id, [FromUri] string persistenceMethod)
        {
            var book = store.GetBookById(id, persistenceMethod);
            if (book == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = $"No such Book with id {id}" });
            return Request.CreateResponse(HttpStatusCode.OK, new { result = book });
        }

        [HttpPut]
        [Route("book")]
        public HttpResponseMessage UpdateBookPrice([FromUri] int id, [FromUri] int price)
        {
            try
            {
                store.UpdateBookPrice(id, price);
                return Request.CreateResponse(HttpStatusCode.OK, new { result = "Price updated successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, new { errorMessage = ex.Message });
            }
        }

        [HttpDelete]
        [Route("book")]
        public HttpResponseMessage DeleteBook([FromUri] int id)
        {
            try
            {
                store.DeleteBook(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { result = "Book deleted successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = ex.Message });
            }
        }
    }
}