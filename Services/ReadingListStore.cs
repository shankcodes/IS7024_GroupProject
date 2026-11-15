using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IS7024_GroupProject.Services;

public sealed class ReadingListStore(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<ReadingList>> GetForUserAsync(int userId)
    {
        return await dbContext.ReadingLists
            .AsNoTracking()
            .Include(readingList => readingList.Book)
            .Where(readingList => readingList.UserId == userId)
            .OrderBy(readingList => readingList.Book.Title)
            .ToListAsync();
    }

    public async Task<bool> AddBookAsync(
        int userId,
        string title,
        string author,
        string googleBooksId,
        CancellationToken cancellationToken = default)
    {
        var book = await dbContext.Books
            .SingleOrDefaultAsync(existingBook => existingBook.GoogleBooksId == googleBooksId, cancellationToken);

        if (book is null)
        {
            book = new Book
            {
                Title = title,
                Author = author,
                GoogleBooksId = googleBooksId
            };

            dbContext.Books.Add(book);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var alreadyOnList = await dbContext.ReadingLists
            .AnyAsync(
                readingList => readingList.UserId == userId && readingList.BookId == book.BookId,
                cancellationToken);

        if (alreadyOnList)
        {
            return false;
        }

        dbContext.ReadingLists.Add(new ReadingList
        {
            UserId = userId,
            BookId = book.BookId,
            Status = ReadingStatus.WantToRead
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}