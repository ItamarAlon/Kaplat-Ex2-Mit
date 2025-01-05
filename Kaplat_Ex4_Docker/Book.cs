// Models/Book.cs
using System.Collections.Generic;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public int Price { get; set; }
        public List<string> Genres { get; set; }
    }
}
