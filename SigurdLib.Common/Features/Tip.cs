using System;

namespace Sigurd.Common.Features
{
    /// <summary>
    /// Contains <see cref="SPlayer"/> tip info.
    /// </summary>
    public class Tip : IComparable<Tip>
    {
        /// <summary>
        /// Gets the <see cref="Tip"/>'s header.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Gets the <see cref="Tip"/>'s message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the <see cref="Tip"/>'s duration.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Gets the <see cref="Tip"/>'s time left.
        /// </summary>
        public float TimeLeft { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Tip"/>'s priority.
        /// </summary>
        public int Priority { get; } = 0;

        /// <summary>
        /// Gets whether or not the <see cref="Tip"/> is a warning. 
        /// </summary>
        public bool IsWarning { get; } = false;

        /// <summary>
        /// Gets whether or not the <see cref="Tip"/> should use saving.
        /// </summary>
        public bool UseSave { get; } = false;

        /// <summary>
        /// Gets the <see cref="Tip"/>'s preference key.
        /// </summary>
        public string PreferenceKey { get; } = "LC_Tip1";

        internal int TipId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tip"/> class.
        /// </summary>
        /// <param name="header"><inheritdoc cref="Header"/></param>
        /// <param name="message"><inheritdoc cref="Message"/></param>
        /// <param name="duration"><inheritdoc cref="Duration"/></param>
        /// <param name="priority"><inheritdoc cref="Priority"/></param>
        /// <param name="isWarning"><inheritdoc cref="IsWarning"/></param>
        /// <param name="useSave"><inheritdoc cref="UseSave"/></param>
        /// <param name="prefsKey"><inheritdoc cref="PreferenceKey"/></param>
        /// <param name="tipId">The <see cref="Tip"/>'s id.</param>
        public Tip(string header, string message, float duration, int priority, bool isWarning, bool useSave, string prefsKey, int tipId)
        {
            Header = header;
            Message = message;
            Duration = duration;
            TimeLeft = duration;
            Priority = priority;

            IsWarning = isWarning;
            UseSave = useSave;
            PreferenceKey = prefsKey;

            TipId = tipId;
        }

        /// <summary>
        /// Compares this <see cref="Tip"/> to another.
        /// </summary>
        /// <param name="other">The other <see cref="Tip"/>.</param>
        /// <returns><see langword="1"/> if <paramref name="other"/> has a higher priority, <see langword="-1"/> if <paramref name="other"/> has a lower priority.
        /// Otherwise returns the difference between the two <see cref="Tip"/>'s ids.
        /// </returns>
        public int CompareTo(Tip other)
        {
            int diff = other.Priority - Priority;

            if (diff < 0) return -1;

            if (diff > 0) return 1;

            return TipId - other.TipId;
        }
    }
}
