using Sigurd.ServerAPI.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigurd.ServerAPI.Extensions
{
    /// <summary>
    /// Useful item extensions.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Gets a <see cref="Common.Features.SItem"/>'s <see cref="ItemNetworking"/>.
        /// </summary>
        /// <param name="item">The <see cref="Common.Features.SItem"/>.</param>
        /// <returns>The <see cref="Common.Features.SItem"/>'s <see cref="ItemNetworking"/></returns>
        public static ItemNetworking GetNetworking(this Common.Features.SItem item)
        {
            return (ItemNetworking)item;
        }
    }
}
