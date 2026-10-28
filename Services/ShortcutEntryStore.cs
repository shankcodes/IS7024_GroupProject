using IS7024_GroupProject.Models;

namespace IS7024_GroupProject.Services;

public sealed class ShortcutEntryStore
{
    private readonly Lock _lock = new();
    private readonly List<ShortcutEntry> _entries = [];

    public IReadOnlyList<ShortcutEntry> GetAll()
    {
        lock (_lock)
        {
            return _entries.ToList();
        }
    }

    public void Add(ShortcutEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }
    }
}
