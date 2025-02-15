using System.Collections.Concurrent;

namespace QBitHelper.Services;

public static class JobStateStore
{
    private static readonly ConcurrentDictionary<string, object> StateStore = new();

    public static void Set<T>(string key, T value)
        where T : notnull
    {
        StateStore[key] = value;
    }

    public static T? Get<T>(string key)
    {
        return StateStore.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }

    public static bool ContainsKey(string key)
    {
        return StateStore.ContainsKey(key);
    }

    public static bool Remove(string key)
    {
        return StateStore.TryRemove(key, out _);
    }
}
