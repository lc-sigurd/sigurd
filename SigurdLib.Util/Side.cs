using System;

namespace Sigurd.Util;

/// <summary>
/// Extension methods providing functionality for <see cref="Side"/>.
/// </summary>
public static class SideExtensions
{
    /// <summary>
    /// Determines whether the <see cref="Side"/> represents a client.
    /// </summary>
    /// <param name="side">A <see cref="Side"/> to test.</param>
    /// <returns><see langword="true"/> if the <see cref="Side"/> includes client-sidedness. Otherwise, <see langword="false"/>.</returns>
    public static bool IsClient(this Side side) => side.ContainsSide(Side.Client);

    /// <summary>
    /// Determines whether the <see cref="Side"/> represents a server.
    /// </summary>
    /// <param name="side"></param>
    /// <returns><see langword="true"/> if the <see cref="Side"/> includes server-sidedness. Otherwise, <see langword="false"/>.</returns>
    public static bool IsServer(this Side side) => side.ContainsSide(Side.Server);

    /// <summary>
    /// Determines whether the <see cref="Side"/> represents a client host, also known as a 'listen server'.
    /// See the <a href="https://docs-multiplayer.unity3d.com/netcode/1.5.2/terms-concepts/network-topologies/#client-hosted-listen-server">Netcode docs</a>
    /// for the definition of a client host.
    /// </summary>
    /// <param name="side"></param>
    /// <returns><see langword="true"/> if the <see cref="Side"/> includes both client- and server-sidedness. Otherwise, <see langword="false"/>.</returns>
    public static bool IsHost(this Side side) => side.ContainsSide(Side.Client | Side.Server);

    private static bool ContainsSide(this Side side, Side testSide) => (side & testSide) == testSide;
}

/// <summary>
/// A networking 'sidedness' of the Lethal Company game.
/// </summary>
[Flags]
public enum Side
{
    /// <summary>
    /// The client-side. See <see cref="NetworkManager.IsClient"/>.
    /// </summary>
    Client = 1 << 0,

    /// <summary>
    /// The server-side. See <see cref="NetworkManager.IsServer"/>.
    /// </summary>
    Server = 1 << 1,
}
