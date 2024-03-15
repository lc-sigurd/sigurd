// The contents of this file are largely based upon https://github.com/grumpydev/TinyMessenger/blob/2893aa72c9533c70dd40866e367f66ac1bf4e79b/src/TinyMessenger/TinyMessenger/TinyMessenger.cs.
// https://github.com/grumpydev licenses the basis of this file to the Sigurd Team under the Microsoft Public License.
// The Sigurd Team licenses this file to you under the LGPL-3.0-or-later license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Sigurd.EventBus.Api;

public class Event
{
    public bool IsCancellable => EventTreeNode.IsCancellable;

    public bool HasResult => EventTreeNode.HasResult;

    internal bool _isCancelled = false;

    private EventPriority? _currentPhase = null;

    public EventTreeNode EventTreeNode => EventTree.Instance.GetNode(GetType());

    [DisallowNull]
    public EventPriority? CurrentPhase {
        get => _currentPhase;
        set {
            if (value is null) throw new ArgumentNullException(nameof(value));
            var current = _currentPhase?.Ordinal ?? -1;
            if (current >= value.Ordinal) throw new ArgumentOutOfRangeException(nameof(value), $"Attempted to set event phase to {value} when already {_currentPhase}");
            _currentPhase = value;
        }
    }
}
