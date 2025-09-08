namespace LibraryCore;
public class Borrower
{
    public string Name { get; }
    public string LibraryCardNumber { get; }
    public List<Book> BorrowedBooks { get; } = new();

    public Borrower(string name, string libraryCardNumber)
    {
        Name = name;
        LibraryCardNumber = libraryCardNumber;
    }

    public void BorrowBook(Book book)
    {
        book.Borrow();
        BorrowedBooks.Add(book);
    }

    public void ReturnBook(Book book)
    {
        book.Return();
        BorrowedBooks.Remove(book);
    }
}