namespace IS7024_GroupProject.Models;

public sealed class ReadingList
{
    public int ReadingListId { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;

    public DateTime? FinishedDate { get; set; }

    public User User { get; set; } = null!;

    public Book Book { get; set; } = null!;
}