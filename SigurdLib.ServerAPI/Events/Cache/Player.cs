using System.Collections.Generic;

namespace Sigurd.ServerAPI.Events.Cache
{
    internal static class Player
    {
        internal static List<ulong> ConnectedPlayers { get; private set; } = new List<ulong>();
    }
}
