namespace Sigurd.ServerAPI.Events.EventArgs.Player;

/// <summary>
/// Contains all the information before a <see cref="Features.Player"/> grabs an <see cref="Features.Item"/>.
/// </summary>
public class GrabbingItemEventArgs : System.EventArgs
{
    /// <summary>
    /// Gets the <see cref="Features.Player"/> that is grabbing the <see cref="Item"/>.
    /// </summary>
    public Features.Player Player { get; }

    /// <summary>
    /// Gets the <see cref="Features.Item"/> that is being grabbed.
    /// </summary>
    public Features.Item Item { get; }

    /// <summary>
    /// Gets or sets whether or not the <see cref="Item"/> can be grabbed.
    /// </summary>
    public bool IsAllowed { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="GrabbingItemEventArgs"/> class.
    /// </summary>
    /// <param name="player"><inheritdoc cref="Player"/></param>
    /// <param name="item"><inheritdoc cref="Item"/></param>
    public GrabbingItemEventArgs(Features.Player player, Features.Item item)
    {
        Player = player;
        Item = item;
    }
}