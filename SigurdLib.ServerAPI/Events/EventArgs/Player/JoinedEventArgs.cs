﻿namespace Sigurd.ServerAPI.Events.EventArgs.Player;

/// <summary>
/// Contains all the information after a <see cref="Common.Features.SPlayer"/> joined the server. Including the host.
/// </summary>
public class JoinedEventArgs : System.EventArgs
{
    /// <summary>
    /// Gets the joined player.
    /// </summary>
    public Common.Features.SPlayer Player { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinedEventArgs"/> class.
    /// </summary>
    /// <param name="player"><inheritdoc cref="Player" /></param>
    public JoinedEventArgs(Common.Features.SPlayer player)
    {
        Player = player;
    }
}