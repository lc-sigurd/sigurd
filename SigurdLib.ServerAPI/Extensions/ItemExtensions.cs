using Sigurd.ServerAPI.Features;

namespace Sigurd.ServerAPI.Extensions;

/// <summary>
/// Useful item extensions.
/// </summary>
public static class ItemExtensions
{
    /// <summary>
    /// Gets a <see cref="Common.Features.SItem"/>'s <see cref="SItemNetworking"/>.
    /// </summary>
    /// <param name="item">The <see cref="Common.Features.SItem"/>.</param>
    /// <returns>The <see cref="Common.Features.SItem"/>'s <see cref="SItemNetworking"/></returns>
    public static SItemNetworking GetNetworking(this Common.Features.SItem item)
    {
        return (SItemNetworking)item;
    }
}