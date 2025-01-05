using BookStore.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BookStore.Controllers
{
    public class BooksController : ApiController
    {
        private static readonly BookStore.Utils.BookStore store = new BookStore.Utils.BookStore();
        private static int requestCounter = 0;
        private static readonly Logger BooksLogger = LogManager.GetLogger("books-logger");
        private static readonly Logger RequestLogger = LogManager.GetLogger("request-logger");

        public override async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            int currentRequestNumber = Interlocked.Increment(ref requestCounter);
            MappedDiagnosticsLogicalContext.Set("RequestNumber", currentRequestNumber);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage response = null;

            RequestLogger.Info($"Incoming request | #{currentRequestNumber} | resource: {controllerContext.Request.RequestUri.AbsolutePath} | HTTP Verb {controllerContext.Request.Method.Method.ToUpper()}");

            try
            {
                response = await base.ExecuteAsync(controllerContext, cancellationToken);
            }
            catch (Exception ex)
            {
                RequestLogger.Error(ex, $"Request #{currentRequestNumber} failed: {ex.Message}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                RequestLogger.Debug($"Request #{currentRequestNumber} duration: {stopwatch.ElapsedMilliseconds}ms");
            }

            return response;
        }

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
            int currentRequestNumber = requestCounter;
            try
            {
                int numOfBooksBeforeAdding = store.GetTotalBooks();
                Book book = store.AddBook(newBook);
                BooksLogger.Info($"Creating new Book with Title [{newBook.Title}]");
                BooksLogger.Debug($"Currently there are {numOfBooksBeforeAdding} Books in the system. New Book will be assigned with id {book.Id}");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = book.Id });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                return Request.CreateResponse(HttpStatusCode.Conflict, new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("books/total")]
        public HttpResponseMessage GetTotalBooks([FromUri] string author = null,
            [FromUri(Name = "price-bigger-than")] int? priceBiggerThan = null,
            [FromUri(Name = "price-less-than")] int? priceLessThan = null,
            [FromUri(Name = "year-bigger-than")] int? yearBiggerThan = null,
            [FromUri(Name = "year-less-than")] int? yearLessThan = null,
            [FromUri] string genres = null)
        {
            int currentRequestNumber = requestCounter;
            try
            {
                List<string> genresList = genres?.Split(',').ToList();
                int totalBooks = store.GetTotalBooks(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genresList);
                BooksLogger.Info($"Total Books found for requested filters is {totalBooks}");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = totalBooks });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("books")]
        public HttpResponseMessage GetBooks([FromUri] string author = null,
            [FromUri(Name = "price-bigger-than")] int? priceBiggerThan = null,
            [FromUri(Name = "price-less-than")] int? priceLessThan = null,
            [FromUri(Name = "year-bigger-than")] int? yearBiggerThan = null,
            [FromUri(Name = "year-less-than")] int? yearLessThan = null,
            [FromUri] string genres = null)
        {
            int currentRequestNumber = requestCounter;
            try
            {
                List<string> genresList = genres?.Split(',').ToList();
                List<Book> books = store.GetBooks(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genresList);
                BooksLogger.Info($"Total Books found for requested filters is {books.Count}");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = books });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("book")]
        public HttpResponseMessage GetBookById([FromUri] int id)
        {
            int currentRequestNumber = requestCounter;
            try
            {
                Book book = store.GetBookById(id);
                if (book == null)
                {
                    string noBookError = $"Error: no such Book with id {id}";
                    BooksLogger.Error(noBookError);
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = noBookError });
                }

                BooksLogger.Debug($"Fetching book id {id} details");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = book });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = ex.Message });
            }
        }


        [HttpPut]
        [Route("book")]
        public HttpResponseMessage UpdateBookPrice([FromUri] int id, [FromUri] int price)
        {
            int currentRequestNumber = requestCounter;
            try
            {
                int oldPrice = store.UpdateBookPrice(id, price);
                BooksLogger.Info($"Update Book id [{id}] price to {price}");
                BooksLogger.Debug($"Book [{store.GetBookById(id).Title}] price change: {oldPrice} --> {price}");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = oldPrice });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                if (ex is KeyNotFoundException)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = ex.Message });
                return Request.CreateResponse(HttpStatusCode.Conflict, new { errorMessage = ex.Message });
            }
        }

        [HttpDelete]
        [Route("book")]
        public HttpResponseMessage DeleteBook([FromUri] int id)
        {
            int currentRequestNumber = requestCounter;
            try
            {
                Book book = store.GetBookById(id);
                int remainingBooksAmount = store.DeleteBook(id);
                BooksLogger.Info($"Removing book [{book.Title}]");
                BooksLogger.Debug($"After removing book [{book.Title}] id: [{id}] there are {remainingBooksAmount} books in the system");
                return Request.CreateResponse(HttpStatusCode.OK, new { result = remainingBooksAmount });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = ex.Message });
            }
        }
    }
}
