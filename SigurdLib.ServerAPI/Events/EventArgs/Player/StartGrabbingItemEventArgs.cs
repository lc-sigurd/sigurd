namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    /// <summary>
    /// Contains all the information before a <see cref="Common.Features.Player"/> starts grabbing an <see cref="Common.Features.Item"/>.
    /// This is when the pickup circle appears.
    /// </summary>
    public class StartGrabbingItemEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Common.Features.Player"/> that is starting to grab the <see cref="Item"/>.
        /// </summary>
        public Common.Features.Player Player { get; }

        /// <summary>
        /// Gets the <see cref="Common.Features.Item"/> that will be grabbed.
        /// </summary>
        public Common.Features.Item Item { get; }

        /// <summary>
        /// Gets or sets whether or not the <see cref="Item"/> is allowed to start being grabbed.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartGrabbingItemEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        /// <param name="item"><inheritdoc cref="Item"/></param>
        public StartGrabbingItemEventArgs(Common.Features.Player player, Common.Features.Item item)
        {
            Player = player;
            Item = item;
        }
    }
}
