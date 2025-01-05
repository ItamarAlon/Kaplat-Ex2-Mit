using BookStore.Models;
using BookStore.Utils;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BookStore.Controllers
{
    [ApiController]
    public class BooksController : ControllerBase
    {
        private static readonly BookStore.Utils.BookStore store = new BookStore.Utils.BookStore();
        private static readonly Logger BooksLogger = LogManager.GetLogger("books-logger");

        [HttpGet]
        [Route("books/health")]
        public IActionResult Health()
        {
            return Ok("OK");
        }

        [HttpPost("book")]
        public IActionResult CreateBook([FromBody] Book newBook)
        {
            try
            {
                int numOfBooksBeforeAdding = store.GetTotalBooks();
                Book book = store.AddBook(newBook);
                BooksLogger.Info($"Creating new Book with Title [{newBook.Title}]");
                BooksLogger.Debug($"Currently there are {numOfBooksBeforeAdding} Books in the system. New Book will be assigned with id {book.Id}");
                return Ok(new { result = book.Id });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                return Conflict(new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [Route("books/total")]
        public IActionResult GetTotalBooks([FromQuery] string? author = null,
            [FromQuery(Name = "price-bigger-than")] int? priceBiggerThan = null,
            [FromQuery(Name = "price-less-than")] int? priceLessThan = null,
            [FromQuery(Name = "year-bigger-than")] int? yearBiggerThan = null,
            [FromQuery(Name = "year-less-than")] int? yearLessThan = null,
            [FromQuery] string? genres = null)
        {
            try
            {
                List<string> genresList = genres?.Split(',').ToList();
                int totalBooks = store.GetTotalBooks(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genresList);
                BooksLogger.Info($"Total Books found for requested filters is {totalBooks}");
                return Ok(new { result = totalBooks });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [HttpGet("books")]
        public IActionResult GetBooks([FromQuery] string? author = null,
            [FromQuery(Name = "price-bigger-than")] int? priceBiggerThan = null,
            [FromQuery(Name = "price-less-than")] int? priceLessThan = null,
            [FromQuery(Name = "year-bigger-than")] int? yearBiggerThan = null,
            [FromQuery(Name = "year-less-than")] int? yearLessThan = null,
            [FromQuery] string? genres = null)
        {
            try
            {
                List<string> genresList = genres?.Split(',').ToList();
                List<Book> books = store.GetBooks(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genresList);
                BooksLogger.Info($"Total Books found for requested filters is {books.Count}");
                return Ok(new { result = books });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [HttpGet("book")]
        public IActionResult GetBookById([FromQuery] int id)
        {
            try
            {
                Book book = store.GetBookById(id);
                if (book == null)
                {
                    string noBookError = $"Error: no such Book with id {id}";
                    BooksLogger.Error(noBookError);
                    return NotFound(new { errorMessage = noBookError });
                }

                BooksLogger.Debug($"Fetching book id {id} details");
                return Ok(new { result = book });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [HttpPut("book")]
        public IActionResult UpdateBookPrice([FromQuery] int id, [FromQuery] int price)
        {
            try
            {
                int oldPrice = store.UpdateBookPrice(id, price);
                BooksLogger.Info($"Update Book id [{id}] price to {price}");
                BooksLogger.Debug($"Book [{store.GetBookById(id).Title}] price change: {oldPrice} --> {price}");
                return Ok(new { result = oldPrice });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                if (ex is KeyNotFoundException)
                    return NotFound(new { errorMessage = ex.Message });
                return Conflict(new { errorMessage = ex.Message });
            }
        }

        [HttpDelete("book")]
        public IActionResult DeleteBook([FromQuery] int id)
        {
            try
            {
                Book book = store.GetBookById(id);
                int remainingBooksAmount = store.DeleteBook(id);
                BooksLogger.Info($"Removing book [{book.Title}]");
                BooksLogger.Debug($"After removing book [{book.Title}] id: [{id}] there are {remainingBooksAmount} books in the system");
                return Ok(new { result = remainingBooksAmount });
            }
            catch (Exception ex)
            {
                BooksLogger.Error(ex, ex.Message);
                return NotFound(new { errorMessage = ex.Message });
            }
        }
    }
}
