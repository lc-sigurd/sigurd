namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    /// <summary>
    /// Contains all the information right before a <see cref="Common.Features.SPlayer"/> leaves the server. Including the local client.
    /// </summary>
    public class LeftEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the player that is leaving.
        /// </summary>
        public Common.Features.SPlayer Player { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftEventArgs"/> class.
        /// </summary>
        /// <param name="player"><inheritdoc cref="Player" /></param>
        public LeftEventArgs(Common.Features.SPlayer player)
        {
            Player = player;
        }
    }
}
