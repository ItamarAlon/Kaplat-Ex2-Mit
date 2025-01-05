// Utils/BookStore.cs
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Utils
{
    public class BookStore
    {
        private List<Book> books;
        private int nextId;

        public BookStore()
        {
            books = new List<Book>();
            nextId = 1;
        }

        public Book AddBook(Book newBook)
        {
            if (books.Any(b => b.Title.Equals(newBook.Title, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Error: Book with the title [{newBook.Title}] already exists in the system");
            if (newBook.Year < 1940 || newBook.Year > 2100)
                throw new ArgumentOutOfRangeException($"Error: Can't create new Book that its year [{newBook.Year}] is not in the accepted range [1940 -> 2100]");
            if (newBook.Price < 0)
                throw new ArgumentOutOfRangeException($"Error: Can't create new Book with negative price");

            newBook.Id = nextId++;
            books.Add(newBook);
            return newBook;
        }

        public int GetTotalBooks(string author = null, int? priceBiggerThan = null, int? priceLessThan = null,
                                  int? yearBiggerThan = null, int? yearLessThan = null, List<string> genres = null)
        {
            var filteredBooks = getBooksHelper(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genres);
            return filteredBooks.Count();
        }

        public List<Book> GetBooks(string author = null, int? priceBiggerThan = null, int? priceLessThan = null,
                                   int? yearBiggerThan = null, int? yearLessThan = null, List<string> genres = null)
        {
            var filteredBooks = getBooksHelper(author, priceBiggerThan, priceLessThan, yearBiggerThan, yearLessThan, genres);
            return filteredBooks.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private IQueryable<Book> getBooksHelper(string author = null, int? priceBiggerThan = null, int? priceLessThan = null,
                                   int? yearBiggerThan = null, int? yearLessThan = null, List<string> genres = null)
        {
            var filteredBooks = books.AsQueryable();

            if (!string.IsNullOrEmpty(author))
                filteredBooks = filteredBooks.Where(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase));
            if (priceBiggerThan.HasValue)
                filteredBooks = filteredBooks.Where(b => b.Price >= priceBiggerThan.Value);
            if (priceLessThan.HasValue)
                filteredBooks = filteredBooks.Where(b => b.Price <= priceLessThan.Value);
            if (yearBiggerThan.HasValue)
                filteredBooks = filteredBooks.Where(b => b.Year >= yearBiggerThan.Value);
            if (yearLessThan.HasValue)
                filteredBooks = filteredBooks.Where(b => b.Year <= yearLessThan.Value);
            if (genres != null && genres.Any())
                filteredBooks = filteredBooks.Where(b => b.Genres.Intersect(genres).Any());

            return filteredBooks;
        }

        public Book GetBookById(int id)
        {
            return books.FirstOrDefault(b => b.Id == id);
        }

        public int DeleteBook(int id)
        {
            Book book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                throw new KeyNotFoundException($"Error: no such Book with id {id}");

            books.Remove(book);
            return books.Count;
        }

        public int UpdateBookPrice(int id, int newPrice)
        {
            Book book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                throw new KeyNotFoundException($"Error: no such Book with id {id}");
            if (newPrice <= 0)
                throw new ArgumentOutOfRangeException($"Error: price update for book [{id}] must be a positive integer");

            int oldPrice = book.Price;
            book.Price = newPrice;
            return oldPrice;
        }
    }
}
