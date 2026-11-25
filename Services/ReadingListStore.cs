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
        string? coverUrl = null,
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
                CoverUrl = coverUrl,
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

    public async Task<bool> UpdateStatusAsync(
        int userId,
        int readingListId,
        ReadingStatus status,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
        {
            return false;
        }

        var readingList = await dbContext.ReadingLists
            .SingleOrDefaultAsync(
                entry => entry.ReadingListId == readingListId && entry.UserId == userId,
                cancellationToken);

        if (readingList is null)
        {
            return false;
        }

        readingList.Status = status;
        readingList.FinishedDate = status == ReadingStatus.Finished
            ? DateTime.Today
            : null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveBookAsync(
        int userId,
        int readingListId,
        CancellationToken cancellationToken = default)
    {
        var readingList = await dbContext.ReadingLists
            .SingleOrDefaultAsync(
                entry => entry.ReadingListId == readingListId && entry.UserId == userId,
                cancellationToken);

        if (readingList is null)
        {
            return false;
        }

        dbContext.ReadingLists.Remove(readingList);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}