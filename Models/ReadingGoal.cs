using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace IS7024_GroupProject.Models;

[PrimaryKey(nameof(UserId), nameof(Year))]
public sealed class ReadingGoal
{
    public int UserId { get; set; }

    public int Year { get; set; }

    [Range(1, int.MaxValue)]
    public int TargetBooks { get; set; }

    public User User { get; set; } = null!;
}