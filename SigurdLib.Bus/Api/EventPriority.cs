using System.Collections.Generic;
using Sigurd.Util.Collections.ObjectModel;

namespace Sigurd.Bus.Api;

public class EventPriority
{
    private static readonly LinkedList<EventPriority> Priorities = new();

    public static int Count => Priorities.Count;

    public static EventPriority Highest;
    public static EventPriority High;
    public static EventPriority Normal;
    public static EventPriority Low;
    public static EventPriority Lowest;

    static EventPriority()
    {
        Highest = new EventPriority("Highest");
        High = new EventPriority("High");
        Normal = new EventPriority("Normal");
        Low = new EventPriority("Low");
        Lowest = new EventPriority("Lowest");
    }

    public string Name { get; }

    public int Ordinal { get; }

    private EventPriority(string name)
    {
        Name = name;
        Ordinal = Priorities.Count;
        Priorities.AddLast(this);
    }

    public static IEnumerable<EventPriority> Values => new ReadOnlyAssemblage<EventPriority>(Priorities);
}
