using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Events.EventArgs.Player
{
    public class DroppedItemEventArgs : System.EventArgs
    {
        public Features.Player Player { get; }

        public Features.Item Item { get; }

        public bool Placing { get; }

        public Vector3 TargetPosition { get; }

        public int FloorYRotation { get; }

        public NetworkObject ParentObjectTo { get; }

        public bool MatchRotationOfParent { get; }

        public bool DroppedInShip { get; }

        public DroppedItemEventArgs(Features.Player player, Features.Item item, bool placeObject, Vector3 targetPosition,
            int floorYRotation, NetworkObject parentObjectTo, bool matchRotationOfParent, bool droppedInShip)
        {
            Player = player;
            Item = item;
            Placing = placeObject;
            TargetPosition = targetPosition;
            FloorYRotation = floorYRotation;
            ParentObjectTo = parentObjectTo;
            MatchRotationOfParent = matchRotationOfParent;
            DroppedInShip = droppedInShip;
        }
    }
}
