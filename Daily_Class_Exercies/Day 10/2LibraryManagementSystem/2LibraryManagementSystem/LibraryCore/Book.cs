namespace LibraryCore;
public class Book
{
    public string Title { get; }
    public string Author { get; }
    public string ISBN { get; }
    public bool IsBorrowed { get; private set; }

    public Book(string title, string author, string isbn)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
    }

    public void Borrow() => IsBorrowed = true;
    public void Return() => IsBorrowed = false;
}