using LibraryCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LibraryTests
{
    [TestClass]
    public class LibraryTests
    {
        private Library _library;
        private Book _book;
        private Borrower _borrower;

        [TestInitialize]
        public void Setup()
        {
            _library = new Library();
            _book = new Book("C# Programming", "John Smith", "123-456");
            _borrower = new Borrower("Alice Johnson", "CARD-001");
        }

        [TestMethod]
        public void AddBook_ShouldAddToLibrary()
        {
            _library.AddBook(_book);
            Assert.AreEqual(1, _library.ViewBooks().Count);
        }

        [TestMethod]
        public void RegisterBorrower_ShouldAddToSystem()
        {
            _library.RegisterBorrower(_borrower);
            Assert.AreEqual(1, _library.ViewBorrowers().Count);
        }

        [TestMethod]
        public void BorrowBook_ShouldMarkAsBorrowed()
        {
            // Arrange
            _library.AddBook(_book);
            _library.RegisterBorrower(_borrower);

            // Act
            _library.BorrowBook("123-456", "CARD-001");

            // Assert
            Assert.IsTrue(_book.IsBorrowed);
            Assert.AreEqual(1, _borrower.BorrowedBooks.Count);
        }

        [TestMethod]
        public void ReturnBook_ShouldMarkAsAvailable()
        {
            // Arrange
            BorrowBook_ShouldMarkAsBorrowed(); // First borrow the book

            // Act
            _library.ReturnBook("123-456", "CARD-001");

            // Assert
            Assert.IsFalse(_book.IsBorrowed);
            Assert.AreEqual(0, _borrower.BorrowedBooks.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BorrowBook_WhenAlreadyBorrowed_ShouldThrowException()
        {
            // Arrange
            _library.AddBook(_book);
            _library.RegisterBorrower(_borrower);
            _library.BorrowBook("123-456", "CARD-001"); // First borrow

            // Act (try to borrow again)
            _library.BorrowBook("123-456", "CARD-001");
        }

        [TestMethod]
        public void ViewBooks_ShouldShowAllBooks()
        {
            // Arrange
            _library.AddBook(_book);
            var book2 = new Book("Clean Code", "Robert Martin", "789-012");
            _library.AddBook(book2);

            // Act
            var books = _library.ViewBooks();

            // Assert
            Assert.AreEqual(2, books.Count);
            CollectionAssert.Contains(books, _book);
            CollectionAssert.Contains(books, book2);
        }
    }
}