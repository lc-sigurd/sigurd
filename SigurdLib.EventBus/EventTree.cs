using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sigurd.EventBus.Api;
using Sigurd.Util.Collections.Generic;

namespace Sigurd.EventBus;

public sealed class EventTree
{
    public static EventTree Instance = new();

    private static readonly IEqualityComparer<Type> TypeComparer = new IdentityEqualityComparer<Type>();

    private EventTreeNode _root;

    private int _currentMaxBusId = -1;

    public int CurrentMaxBusId => _currentMaxBusId;

    public int MaxBusCount => CurrentMaxBusId + 1;

    private LinkedList<EventTreeNode> _allNodes = [];

    private readonly ConcurrentDictionary<Type, EventTreeNode> _eventNodes;

    private EventTree()
    {
        _root = EventTreeNode.CreateRoot(typeof(Event), this);
        _allNodes.AddFirst(_root);
        _eventNodes = new([new KeyValuePair<Type, EventTreeNode>(typeof(Event), _root)], TypeComparer);
    }

    internal int GetAndIncrementIdForNewBus()
    {
        var id = Interlocked.Increment(ref _currentMaxBusId);
        ResizeBusArrays(id + 1);
        return id;
    }

    public EventTreeNode GetNode(Type eventType)
    {
        if (TypeComparer.Equals(eventType,typeof(Event))) return _root;
        return _eventNodes.GetOrAdd(eventType, ComputeNode);
    }

    private EventTreeNode ComputeNode(Type eventType)
    {
        if (eventType.IsGenericType)
            throw new NotSupportedException("Generic event types & their subtypes are not supported.");

        Type baseType = eventType.BaseType!;

        if (!TypeComparer.Equals(baseType, typeof(Event)) && eventType.IsAbstract && !baseType.IsAbstract)
            throw new NotSupportedException(
                $"Abstract event type {eventType} has a non-abstract base type {baseType}.\n" +
                $"Base types of abstract event types must be abstract, with the exception of the root {nameof(Event)} type."
            );

        EventTreeNode baseTypeEventTreeNode = GetNode(baseType);
        return baseTypeEventTreeNode.AddChild(baseType);
    }

    internal void ResizeBusArrays(int newCapacity)
    {
        foreach (var eventTreeNode in PreOrderTraverseNodes()) {
            eventTreeNode.ResizeBusEventListenerEnumerableArray(newCapacity);
        }
    }

    internal void ClearBus(int busId)
    {
        foreach (var eventTreeNode in PreOrderTraverseNodes()) {
            eventTreeNode.ClearBus(busId);
        }
    }

    public IEnumerable<EventTreeNode> PreOrderTraverseNodes() => PreOrderTraverseSubtreeNodes(_root);

    protected static IEnumerable<EventTreeNode> PreOrderTraverseSubtreeNodes(EventTreeNode subtreeRoot)
    {
        yield return subtreeRoot;

        foreach (var child in subtreeRoot.Children) {
            foreach (var subtreeNode in PreOrderTraverseSubtreeNodes(child)) {
                yield return subtreeNode;
            }
        }
    }

    public IEnumerable<EventTreeNode> PostOrderTraverseNodes() => PostOrderTraverseSubtreeNodes(_root);

    protected static IEnumerable<EventTreeNode> PostOrderTraverseSubtreeNodes(EventTreeNode subtreeRoot)
    {
        foreach (var child in subtreeRoot.Children) {
            foreach (var subtreeNode in PostOrderTraverseSubtreeNodes(child)) {
                yield return subtreeNode;
            }
        }

        yield return subtreeRoot;
    }
}
