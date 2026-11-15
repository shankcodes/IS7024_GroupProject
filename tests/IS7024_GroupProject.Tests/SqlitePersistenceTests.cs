using System.Data.Common;
using IS7024_GroupProject.Data;
using IS7024_GroupProject.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IS7024_GroupProject.Tests;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public async Task FreshDatabaseMigrationCreatesCoreTables()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using (var context = CreateContext(databasePath))
            {
                await context.Database.MigrateAsync();

                var tableNames = await GetTableNamesAsync(context.Database.GetDbConnection());

                Assert.Contains("Books", tableNames);
                Assert.Contains("ReadingLists", tableNames);
                Assert.Contains("ReadingGoals", tableNames);
            }
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task ReadingListEntryPersistsAfterContextRestart()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using (var context = CreateContext(databasePath))
            {
                await context.Database.MigrateAsync();
                var user = CreateUser(1);
                var book = new Book
                {
                    Title = "The Martian",
                    Author = "Andy Weir",
                    GoogleBooksId = "the-martian-test"
                };

                context.Users.Add(user);
                context.Books.Add(book);
                context.ReadingLists.Add(new ReadingList
                {
                    User = user,
                    Book = book,
                    Status = ReadingStatus.WantToRead
                });

                await context.SaveChangesAsync();
            }

            await using (var restartedContext = CreateContext(databasePath))
            {
                var readingListEntry = await restartedContext.ReadingLists
                    .Include(entry => entry.Book)
                    .SingleAsync();

                Assert.Equal("The Martian", readingListEntry.Book.Title);
                Assert.Equal(ReadingStatus.WantToRead, readingListEntry.Status);
            }
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task DuplicateGoogleBooksIdIsRejected()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using (var context = CreateContext(databasePath))
            {
                await context.Database.MigrateAsync();

                context.Books.Add(new Book
                {
                    Title = "The Martian",
                    Author = "Andy Weir",
                    GoogleBooksId = "duplicate-test"
                });
                await context.SaveChangesAsync();

                context.Books.Add(new Book
                {
                    Title = "Another Title",
                    Author = "Another Author",
                    GoogleBooksId = "duplicate-test"
                });

                await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                Assert.Equal(1, await context.Books.CountAsync());
            }
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    private static ApplicationDbContext CreateContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath
            }.ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateUser(int userId)
    {
        return new User
        {
            Id = userId,
            UserName = $"reader{userId}@example.com",
            NormalizedUserName = $"READER{userId}@EXAMPLE.COM",
            Email = $"reader{userId}@example.com",
            NormalizedEmail = $"READER{userId}@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    private static async Task<HashSet<string>> GetTableNamesAsync(DbConnection connection)
    {
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        await using var reader = await command.ExecuteReaderAsync();
        var tableNames = new HashSet<string>(StringComparer.Ordinal);

        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        await connection.CloseAsync();
        return tableNames;
    }

    private static string CreateDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"booknest-tests-{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabase(string databasePath)
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        var walPath = $"{databasePath}-wal";
        if (File.Exists(walPath))
        {
            File.Delete(walPath);
        }

        var sharedMemoryPath = $"{databasePath}-shm";
        if (File.Exists(sharedMemoryPath))
        {
            File.Delete(sharedMemoryPath);
        }
    }
}
