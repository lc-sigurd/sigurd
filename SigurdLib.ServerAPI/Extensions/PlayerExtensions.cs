using Sigurd.ServerAPI.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigurd.ServerAPI.Extensions
{
    /// <summary>
    /// Useful player extensions.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        /// Gets a <see cref="Common.Features.SPlayer"/>'s <see cref="PlayerNetworking"/>.
        /// </summary>
        /// <param name="player">The <see cref="Common.Features.SPlayer"/>.</param>
        /// <returns>The <see cref="Common.Features.SPlayer"/>'s <see cref="PlayerNetworking"/></returns>
        public static PlayerNetworking GetNetworking(this Common.Features.SPlayer player)
        {
            return (PlayerNetworking)player;
        }
    }
}
