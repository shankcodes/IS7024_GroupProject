using System.ComponentModel.DataAnnotations;

namespace IS7024_GroupProject.Models;

public sealed class ReadingGoal
{
    public int UserId { get; set; }

    public int Year { get; set; }

    [Range(1, int.MaxValue)]
    public int TargetBooks { get; set; }

    public User User { get; set; } = null!;
}