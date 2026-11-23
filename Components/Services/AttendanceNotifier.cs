using System.Collections.Concurrent;
using attendance.Components.Models;

namespace attendance.Services
{
    /// <summary>
    /// Sends attendance changes to every open attendance page, so they update without a refresh.
    /// It also remembers changes that are not saved yet, so a page opened later shows them too.
    /// Note: this lives in server memory, so it only reaches pages on the same server instance.
    /// </summary>
    public class AttendanceNotifier
    {
        // "classname|date" -> BUID -> latest unsaved change
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, StudentData>> unsaved = new();

        /// <summary>Raised with the class/date key, the change, and the id of the page that made it.</summary>
        public event Func<string, StudentData, Guid, Task>? Changed;

        public static string Key(string className, string date) => $"{className}|{date}";

        public async Task PublishAsync(string key, StudentData change, Guid sourceId)
        {
            unsaved.GetOrAdd(key, _ => new())[change.BUID] = change;

            foreach (var handler in Changed?.GetInvocationList().Cast<Func<string, StudentData, Guid, Task>>() ?? [])
            {
                try { await handler(key, change, sourceId); }
                catch (Exception e) { Console.WriteLine($"Live attendance update failed: {e.Message}"); }
            }
        }

        public void ClearUnsaved(string key) => unsaved.TryRemove(key, out _);

        public IEnumerable<StudentData> GetUnsaved(string key) =>
            unsaved.TryGetValue(key, out var changes) ? changes.Values : [];
    }
}
