namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    /// <summary>
    /// Contains all the information before a <see cref="Common.Features.SPlayer"/> grabs an <see cref="Common.Features.SItem"/>.
    /// </summary>
    public class GrabbingItemEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Common.Features.SPlayer"/> that is grabbing the <see cref="Item"/>.
        /// </summary>
        public Common.Features.SPlayer Player { get; }

        /// <summary>
        /// Gets the <see cref="Common.Features.SItem"/> that is being grabbed.
        /// </summary>
        public Common.Features.SItem Item { get; }

        /// <summary>
        /// Gets or sets whether or not the <see cref="Item"/> can be grabbed.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrabbingItemEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        /// <param name="item"><inheritdoc cref="Item"/></param>
        public GrabbingItemEventArgs(Common.Features.SPlayer player, Common.Features.SItem item)
        {
            Player = player;
            Item = item;
        }
    }
}
