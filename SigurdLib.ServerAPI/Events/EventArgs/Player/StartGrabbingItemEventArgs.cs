namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    /// <summary>
    /// Contains all the information before a <see cref="Features.Player"/> starts grabbing an <see cref="Features.Item"/>.
    /// This is when the pickup circle appears.
    /// </summary>
    public class StartGrabbingItemEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Features.Player"/> that is starting to grab the <see cref="Item"/>.
        /// </summary>
        public Features.Player Player { get; }

        /// <summary>
        /// Gets the <see cref="Features.Item"/> that will be grabbed.
        /// </summary>
        public Features.Item Item { get; }

        /// <summary>
        /// Gets or sets whether or not the <see cref="Item"/> is allowed to start being grabbed.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartGrabbingItemEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        /// <param name="item"><inheritdoc cref="Item"/></param>
        public StartGrabbingItemEventArgs(Features.Player player, Features.Item item)
        {
            Player = player;
            Item = item;
        }
    }
}
