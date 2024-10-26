using UnityEngine;

namespace GameClient.Runtime
{
    public class PassiveTile : MonoBehaviour
    {
        public Tile AttachedTile;

        public void Initialize(Tile tile)
        {
            AttachedTile = tile;
        }
    }
}
