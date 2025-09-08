namespace LibraryCore;
public class Library
{
    public List<Book> Books { get; } = new();
    public List<Borrower> Borrowers { get; } = new();

    public void AddBook(Book book) => Books.Add(book);
    public void RegisterBorrower(Borrower borrower) => Borrowers.Add(borrower);

    public void BorrowBook(string isbn, string libraryCardNumber)
    {
        var book = Books.FirstOrDefault(b => b.ISBN == isbn);
        var borrower = Borrowers.FirstOrDefault(b => b.LibraryCardNumber == libraryCardNumber);

        if (book == null || borrower == null || book.IsBorrowed)
            throw new InvalidOperationException("Cannot borrow book");

        borrower.BorrowBook(book);
    }

    public void ReturnBook(string isbn, string libraryCardNumber)
    {
        var book = Books.FirstOrDefault(b => b.ISBN == isbn);
        var borrower = Borrowers.FirstOrDefault(b => b.LibraryCardNumber == libraryCardNumber);

        if (book == null || borrower == null || !book.IsBorrowed)
            throw new InvalidOperationException("Cannot return book");

        borrower.ReturnBook(book);
    }

    public List<Book> ViewBooks() => Books;
    public List<Borrower> ViewBorrowers() => Borrowers;
}