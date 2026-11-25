using Xunit;

namespace IS7024_GroupProject.Tests.Pages;

public sealed class ReadingListPageTests
{
    private static readonly string PagePath = FindPagePath();

    [Fact]
    public void PageContract_ContainsStatusChoicesAndHandler()
    {
        var page = File.ReadAllText(PagePath);

        Assert.Contains("asp-page-handler=\"UpdateStatus\"", page);
        Assert.Contains("value=\"WantToRead\"", page);
        Assert.Contains("value=\"Reading\"", page);
        Assert.Contains("value=\"Finished\"", page);
        Assert.Contains("StatusInput.ReadingListId", page);
        Assert.Contains("StatusInput.Status", page);
    }

    [Fact]
    public void PageContract_ContainsRemoveActionAndExactEmptyState()
    {
        var page = File.ReadAllText(PagePath);

        Assert.Contains("asp-page-handler=\"Remove\"", page);
        Assert.Contains("RemoveInput.ReadingListId", page);
        Assert.Contains("Your list is empty.", page);
        Assert.Contains("<a asp-page=\"/Index\">Search for a book</a>", page);
        Assert.Contains("to get started.", page);
        Assert.Contains("FinishedDate", page);
    }

    private static string FindPagePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Pages", "ReadingList.cshtml")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new FileNotFoundException("Could not locate the ReadingList Razor page.")
            : Path.Combine(directory.FullName, "Pages", "ReadingList.cshtml");
    }
}
