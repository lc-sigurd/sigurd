// The contents of this file are largely based upon https://github.com/MinecraftForge/EventBus/tree/ec01e75bf78ce21b00e4675ca4370d591b9d1457
// Forge Development LLC licenses EventBus to the Sigurd Team under the LGPL-2.1-only license.
// The Sigurd Team licenses this file to you under the LGPL-3.0-or-later license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Sigurd.EventBus.Api;
using Sigurd.Util;
using Sigurd.Util.Collections.ObjectModel;
using Sigurd.Util.Extensions;

namespace Sigurd.EventBus;

public class EventTreeNode
{
    public bool HasResult { get; }

    public bool IsCancellable { get; }

    public Optional<EventTreeNode> Parent { get; private set; } = Optional<EventTreeNode>.None;

    public Type EventType { get; }

    private readonly EventTree _tree;

    private LinkedList<EventTreeNode>? _children;

    private EventListenerEnumerable[] _busEventListenerEnumerables;

    public static EventTreeNode CreateRoot(Type rootEventType, EventTree tree) => new EventTreeNode(rootEventType, tree);

    private EventTreeNode(Type eventType, EventTree tree)
    {
        EventType = eventType;
        _tree = tree;

        _busEventListenerEnumerables = Enumerable.Range(0, _tree.MaxBusCount)
            .Select(busId => new EventListenerEnumerable(this, busId))
            .ToArray();
    }

    public ICollection<EventTreeNode> Children => _children is null
        ? Array.Empty<EventTreeNode>()
        : new ReadOnlyAssemblage<EventTreeNode>(_children);

    public EventTreeNode AddChild(Type childEventType)
    {
        if (!EventType.IsAssignableFrom(childEventType))
            throw new ArgumentException("Child event type is not derived from event type this node represents.");

        var childNode = new EventTreeNode(childEventType, _tree);

        _children ??= new LinkedList<EventTreeNode>();
        _children.AddLast(childNode);
        childNode.Parent = Optional.Some(this);
        return childNode;
    }

    internal void ResizeBusEventListenerEnumerableArray(int newCapacity)
    {
        var oldCapacity = _busEventListenerEnumerables.Length;
        if (oldCapacity >= newCapacity) return;

        var newEventListenerEnumerables = Enumerable.Range(oldCapacity, newCapacity - oldCapacity)
            .Select(busId => new EventListenerEnumerable(this, busId));

        _busEventListenerEnumerables = _busEventListenerEnumerables
            .Concat(newEventListenerEnumerables)
            .ToArray();
    }

    protected EventListenerEnumerable GetBusListenerEnumerable(int busId) => _busEventListenerEnumerables[busId];

    public IEnumerable<IEventListener> GetBusListeners(int busId) => GetBusListenerEnumerable(busId);

    public void RegisterListener(int busId, EventPriority priority, IEventListener listener)
        => GetBusListenerEnumerable(busId).RegisterListener(priority, listener);

    public void UnregisterListener(int busId, IEventListener listener)
        => GetBusListenerEnumerable(busId).UnregisterListener(listener);

    public void ClearBus(int busId) => GetBusListenerEnumerable(busId).Dispose();

    protected sealed class EventListenerEnumerable : IEnumerable<IEventListener>, IDisposable
    {
        private bool _disposed;

        private readonly EventTreeNode _node;
        private readonly int _busId;

        [MemberNotNullWhen(false, nameof(_orderedListenersCache))]
        protected bool CacheDirty { get; private set; }

        private IEventListener[]? _orderedListenersCache;

        private LinkedList<IEventListener>[] _priorityListenerLists = EventPriority.Values
            .Select(_ => new LinkedList<IEventListener>())
            .ToArray();

        private readonly SemaphoreSlim _accessLock = new(1, 1);

        public EventListenerEnumerable(EventTreeNode node, int busId)
        {
            _node = node;
            _busId = busId;
        }

        IEnumerator<IEventListener> IEnumerable<IEventListener>.GetEnumerator()
        {
            if (CacheDirty) {
                _orderedListenersCache = ComputeOrderedListeners();
                CacheDirty = false;
            }

            return _orderedListenersCache
                .AsEnumerable()
                .GetEnumerator();
        }

        public IEnumerator GetEnumerator() => (this as IEnumerable<IEventListener>).GetEnumerator();

        private IEventListener[] ComputeOrderedListeners()
        {
            return EventPriority.Values
                .Select(GetListenersWithPriorityNotifier)
                .SelectManyValueWhereSome()
                .ToArray();

            Optional<IEnumerable<IEventListener>> GetListenersWithPriorityNotifier(EventPriority priority)
                => GetListeners(priority)
                    .Select(listeners => listeners.Prepend(priority));
        }

        private Optional<IEnumerable<IEventListener>> GetListeners(EventPriority priority)
        {
            _accessLock.Wait();
            Optional<IEnumerable<IEventListener>> parentListeners = _node.Parent
                .Select(parent => parent
                    .GetBusListenerEnumerable(_busId)
                    .GetListeners(priority));

            ICollection<IEventListener> thisListeners;
            _accessLock.Wait();
            try {
               thisListeners = _priorityListenerLists[priority.Ordinal];
            }
            finally {
                _accessLock.Release();
            }

            if (parentListeners.IsNone && thisListeners.Count == 0)
                return Optional<IEnumerable<IEventListener>>.None;

            return Optional.Some(parentListeners
                .IfNone(Array.Empty<IEventListener>)
                .Concat(thisListeners));
        }

        protected void SetDirty()
        {
            CacheDirty = true;

            foreach (var eventTreeNode in _node.Children) {
                eventTreeNode
                    .GetBusListenerEnumerable(_busId)
                    .SetDirty();
            }
        }

        public void RegisterListener(EventPriority priority, IEventListener listener)
        {
            _accessLock.Wait();
            try {
                _priorityListenerLists[priority.Ordinal].AddLast(listener);
            }
            finally {
                _accessLock.Release();
            }
        }

        public void UnregisterListener(IEventListener listener)
        {
            _accessLock.Wait();
            try {
                // We want to ensure the listener is removed from all priority lists.
                // As `.All()` is fail-fast, it would only remove the first occurrence of listener across all priority lists.
                // ReSharper disable once ReplaceWithSingleCallToAny
                var listenerRemoved = _priorityListenerLists
                    .Where(listeners => listeners.Remove(listener))
                    .Any();
                if (listenerRemoved) SetDirty();
            }
            finally {
                _accessLock.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) {
                _accessLock.Wait();
                try {
                    foreach (var priorityListenerList in _priorityListenerLists) {
                        priorityListenerList.Clear();
                    }
                    _priorityListenerLists.Initialize();
                    _priorityListenerLists = null!;
                }
                finally {
                    _accessLock.Release();
                }

                _accessLock.Dispose();
                _orderedListenersCache = null;
            }

            _disposed = true;
        }
    }
}
