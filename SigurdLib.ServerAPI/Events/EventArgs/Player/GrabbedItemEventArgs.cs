namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    /// <summary>
    /// Contains all the information after a <see cref="Common.Features.SPlayer"/> grabs an <see cref="Common.Features.SItem"/>.
    /// </summary>
    public class GrabbedItemEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Common.Features.SPlayer"/> that grabbed the <see cref="Item"/>.
        /// </summary>
        public Common.Features.SPlayer Player { get; }

        /// <summary>
        /// Gets the <see cref="Common.Features.SItem"/> that was grabbed.
        /// </summary>
        public Common.Features.SItem Item { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrabbedItemEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        /// <param name="item"><inheritdoc cref="Item"/></param>
        public GrabbedItemEventArgs(Common.Features.SPlayer player, Common.Features.SItem item)
        {
            Player = player;
            Item = item;
        }
    }
}
