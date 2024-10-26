using CerberusFramework.Core.MVC;
using GameClient.GameData;
using UnityEngine;

namespace GameClient.Runtime.PlayerUnits
{
    public class PlayerUnitData : Data
    {
        public PlayerUnitDataHolder PlayerUnitDataHolder { get; private set; }
        public Tile AttachedTile { get; private set; }
        public Vector3 Position { get; private set; }
        public Tile LookAtTile { get; private set; }

        public PlayerUnitData(PlayerUnitDataHolder playerUnitDataHolder, Tile attachedTile, Vector3 position, Tile lookAtTile)
        {
            PlayerUnitDataHolder = playerUnitDataHolder;
            AttachedTile = attachedTile;
            Position = position;
            LookAtTile = lookAtTile;
        }
    }
}
