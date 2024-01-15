using Sigurd.ServerAPI.Features;

namespace Sigurd.ServerAPI.Extensions;

/// <summary>
/// Useful player extensions.
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    /// Gets a <see cref="Common.Features.SPlayer"/>'s <see cref="SPlayerNetworking"/>.
    /// </summary>
    /// <param name="player">The <see cref="Common.Features.SPlayer"/>.</param>
    /// <returns>The <see cref="Common.Features.SPlayer"/>'s <see cref="SPlayerNetworking"/></returns>
    public static SPlayerNetworking GetNetworking(this Common.Features.SPlayer player)
    {
        return (SPlayerNetworking)player;
    }
}