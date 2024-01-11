namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    public class GrabbedItemEventArgs : System.EventArgs
    {
        public Features.Player Player { get; }

        public Features.Item Item { get; }

        public GrabbedItemEventArgs(Features.Player player, Features.Item item)
        {
            Player = player;
            Item = item;
        }
    }
}
