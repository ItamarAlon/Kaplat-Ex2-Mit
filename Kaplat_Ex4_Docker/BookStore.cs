using BookStore.Models;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Utils
{
    public class PostgresContext : DbContext
    {
        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Username=postgres;Password=docker;Database=books;");
        }
    }

    public class BookStore
    {
        private readonly PostgresContext m_postgresContext;
        private readonly IMongoCollection<Book> m_mongoCollection;

        public BookStore()
        {
            m_postgresContext = new PostgresContext();
            var mongoClient = new MongoClient("mongodb://mongo:27017");
            var database = mongoClient.GetDatabase("books");
            m_mongoCollection = database.GetCollection<Book>("books");
        }

        public Book AddBook(Book newBook)
        {
            ValidateBook(newBook);
            m_postgresContext.Books.Add(newBook);
            m_postgresContext.SaveChanges();
            m_mongoCollection.InsertOne(newBook);
            return newBook;
        }

        public int GetTotalBooks(string persistenceMethod)
        {
            return persistenceMethod == "POSTGRES" ? m_postgresContext.Books.Count() : (int)m_mongoCollection.CountDocuments(_ => true);
        }

        public Book GetBookById(int id, string persistenceMethod)
        {
            return persistenceMethod == "POSTGRES"
                ? m_postgresContext.Books.FirstOrDefault(b => b.Id == id)
                : m_mongoCollection.Find(b => b.Id == id).FirstOrDefault();
        }

        public void UpdateBookPrice(int id, int newPrice)
        {
            if (newPrice <= 0) throw new ArgumentOutOfRangeException("Price must be positive.");
            var bookPostgres = m_postgresContext.Books.FirstOrDefault(b => b.Id == id);
            var filter = Builders<Book>.Filter.Eq(b => b.Id, id);
            var update = Builders<Book>.Update.Set(b => b.Price, newPrice);

            if (bookPostgres != null)
            {
                bookPostgres.Price = newPrice;
                m_postgresContext.SaveChanges();
                m_mongoCollection.UpdateOne(filter, update);
            }
        }

        public void DeleteBook(int id)
        {
            var bookPostgres = m_postgresContext.Books.FirstOrDefault(b => b.Id == id);
            var filter = Builders<Book>.Filter.Eq(b => b.Id, id);

            if (bookPostgres != null)
            {
                m_postgresContext.Books.Remove(bookPostgres);
                m_postgresContext.SaveChanges();
                m_mongoCollection.DeleteOne(filter);
            }
        }

        private void ValidateBook(Book book)
        {
            if (book.Year < 1940 || book.Year > 2100)
                throw new ArgumentOutOfRangeException("Year out of range.");
            if (book.Price < 0)
                throw new ArgumentOutOfRangeException("Price must be positive.");
        }
    }
}