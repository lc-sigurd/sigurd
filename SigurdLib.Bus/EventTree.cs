using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Sigurd.Bus.Api;
using Sigurd.Util.Collections.Generic;

namespace Sigurd.Bus;

public sealed class EventTree
{
    private static readonly IEqualityComparer<Type> TypeComparer = new IdentityEqualityComparer<Type>();

    private readonly EventTreeNode _root;

    private readonly LinkedList<EventTreeNode> _allNodes = [];

    private readonly ConcurrentDictionary<Type, EventTreeNode> _eventNodes;

    internal EventTree()
    {
        _root = EventTreeNode.CreateRoot(typeof(Event), this);
        _allNodes.AddFirst(_root);
        _eventNodes = new([new KeyValuePair<Type, EventTreeNode>(typeof(Event), _root)], TypeComparer);
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

        if (eventType.IsAbstract && !baseType.IsAbstract)
            throw new NotSupportedException(
                $"Abstract event type {eventType} has a non-abstract base type {baseType}.\n" +
                $"Base types of abstract event types must be abstract, with the exception of the root {nameof(Event)} type."
            );

        EventTreeNode baseTypeEventTreeNode = GetNode(baseType);
        return baseTypeEventTreeNode.AddChild(baseType);
    }

    internal void ClearBus(int busId)
    {
        foreach (var eventTreeNode in PreOrderTraverseNodes()) {
            eventTreeNode.Clear();
        }
    }

    public IEnumerable<EventTreeNode> PreOrderTraverseNodes() => PreOrderTraverseSubtreeNodes(_root);

    private static IEnumerable<EventTreeNode> PreOrderTraverseSubtreeNodes(EventTreeNode subtreeRoot)
    {
        yield return subtreeRoot;

        foreach (var child in subtreeRoot.Children) {
            foreach (var subtreeNode in PreOrderTraverseSubtreeNodes(child)) {
                yield return subtreeNode;
            }
        }
    }

    public IEnumerable<EventTreeNode> PostOrderTraverseNodes() => PostOrderTraverseSubtreeNodes(_root);

    private static IEnumerable<EventTreeNode> PostOrderTraverseSubtreeNodes(EventTreeNode subtreeRoot)
    {
        foreach (var child in subtreeRoot.Children) {
            foreach (var subtreeNode in PostOrderTraverseSubtreeNodes(child)) {
                yield return subtreeNode;
            }
        }

        yield return subtreeRoot;
    }
}
