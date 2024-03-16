// The contents of this file are largely based upon https://github.com/MinecraftForge/EventBus/tree/ec01e75bf78ce21b00e4675ca4370d591b9d1457
// Forge Development LLC licenses EventBus to the Sigurd Team under the LGPL-2.1-only license.
// The Sigurd Team licenses this file to you under the LGPL-3.0-or-later license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Sigurd.Bus.Api;
using Sigurd.Bus.Listener;
using Sigurd.Util;
using Sigurd.Util.Collections.ObjectModel;

namespace Sigurd.Bus;

public class EventTreeNode
{
    public bool HasResult { get; }

    public bool IsCancellable { get; }

    public Optional<EventTreeNode> Parent { get; private set; } = Optional<EventTreeNode>.None;

    public Type EventType { get; }

    private readonly EventTree _tree;

    private LinkedList<EventTreeNode>? _children;

    private readonly EventListenerEnumerable _listeners;

    public static EventTreeNode CreateRoot(Type rootEventType, EventTree tree) => new EventTreeNode(rootEventType, tree);

    private EventTreeNode(Type eventType, EventTree tree)
    {
        EventType = eventType;
        _tree = tree;

        _listeners = new(this);

        IsCancellable = typeof(ICancellableEvent).IsAssignableFrom(eventType);
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

    public IEnumerable<IEventListener> GetListeners() => _listeners;

    public IEnumerable<IEventListener> GetPriorityListeners(EventPriority priority) => _listeners.GetPriorityListeners(priority);

    public void RegisterListener(EventPriority priority, IEventListener listener)
        => _listeners.RegisterListener(priority, listener);

    public void UnregisterListener(IEventListener listener)
        => _listeners.UnregisterListener(listener);

    public void Clear() => _listeners.Dispose();

    protected sealed class EventListenerEnumerable : IEnumerable<IEventListener>, IDisposable
    {
        private bool _disposed;

        private readonly EventTreeNode _node;

        [MemberNotNullWhen(false, nameof(_orderedListenersCache), nameof(_orderedPriorityListenersCache))]
        protected bool CacheDirty { get; private set; } = true;

        private IEventListener[]? _orderedListenersCache;
        private IEventListener[][]? _orderedPriorityListenersCache;

        private LinkedList<IEventListener>[] _priorityListenerLists = EventPriority.Values
            .Select(_ => new LinkedList<IEventListener>())
            .ToArray();

        private readonly SemaphoreSlim _accessLock = new(1, 1);

        public EventListenerEnumerable(EventTreeNode node)
        {
            _node = node;
        }

        IEnumerator<IEventListener> IEnumerable<IEventListener>.GetEnumerator()
        {
            EnsureCleanCache();

            return _orderedListenersCache
                .AsEnumerable()
                .GetEnumerator();
        }

        public IEnumerator GetEnumerator() => (this as IEnumerable<IEventListener>).GetEnumerator();

        public IEnumerable<IEventListener> GetPriorityListeners(EventPriority priority)
        {
            EnsureCleanCache();

            return _orderedPriorityListenersCache[priority.Ordinal];
        }

        [MemberNotNull(nameof(_orderedListenersCache), nameof(_orderedPriorityListenersCache))]
        private void EnsureCleanCache()
        {
            if (!CacheDirty) return;
            BuildCache();
        }

        [MemberNotNull(nameof(_orderedListenersCache), nameof(_orderedPriorityListenersCache))]
        private void BuildCache()
        {
            _orderedPriorityListenersCache = EventPriority.Values
                .Select(GetListeners)
                .Select(value => value.IsSome ? value.ValueUnsafe : Enumerable.Empty<IEventListener>())
                .Select(enumerable => enumerable.ToArray())
                .ToArray();

            _orderedListenersCache = _orderedPriorityListenersCache
                .SelectMany(x => x)
                .ToArray();

            CacheDirty = false;
        }

        private Optional<IEnumerable<IEventListener>> GetListeners(EventPriority priority)
        {
            Optional<IEnumerable<IEventListener>> parentListeners = _node.Parent
                .Select(parent => parent
                    .GetPriorityListeners(priority));

            ICollection<IEventListener> thisListeners;
            _accessLock.Wait();
            try {
                thisListeners = _priorityListenerLists[priority.Ordinal]
                    .Select(Unwrap)
                    .ToArray();
            }
            finally {
                _accessLock.Release();
            }

            if (parentListeners.IsNone && thisListeners.Count == 0)
                return Optional<IEnumerable<IEventListener>>.None;

            return Optional.Some(parentListeners
                .IfNone(Array.Empty<IEventListener>)
                .Concat(thisListeners));

            // If the Event does not implement ICancellable, 'unwrap' it from its predicate
            IEventListener Unwrap(IEventListener listener)
            {
                if (_node.IsCancellable) return listener;
                if (listener is CancellationFilteredEventListener filteredListener)
                    return filteredListener.Inner;
                return listener;
            }
        }

        protected void SetDirty()
        {
            CacheDirty = true;

            foreach (var eventTreeNode in _node.Children) {
                eventTreeNode._listeners.SetDirty();
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
