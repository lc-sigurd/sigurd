namespace Sigurd.ServerAPI.Events.EventArgs.Player;

/// <summary>
/// Contains all the information after a <see cref="Features.Player"/> grabs an <see cref="Features.Item"/>.
/// </summary>
public class GrabbedItemEventArgs : System.EventArgs
{
    /// <summary>
    /// Gets the <see cref="Features.Player"/> that grabbed the <see cref="Item"/>.
    /// </summary>
    public Features.Player Player { get; }

    /// <summary>
    /// Gets the <see cref="Features.Item"/> that was grabbed.
    /// </summary>
    public Features.Item Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrabbedItemEventArgs"/> class.
    /// </summary>
    /// <param name="player"><inheritdoc cref="Player"/></param>
    /// <param name="item"><inheritdoc cref="Item"/></param>
    public GrabbedItemEventArgs(Features.Player player, Features.Item item)
    {
        Player = player;
        Item = item;
    }
}