using UnityEngine;

namespace GameClient.GameData
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "GameClient/LevelData", order = 1)]
    public class EnvironmentData : ScriptableObject
    {
        public string TileKey;
        public string GroundTileKey;
        public string PlayerUnitTileKey;
        public string PathTileKey;
        public string EnemySpawnGateKey;
        public string MainTowerKey;

        public int TileSize = 2;
        public Vector2Int StartingPosition = new(5, 5);
        public int TotalWidth = 20;
        public int TotalHeight = 20;
        public int PlayableWidth = 10;
        public int PlayableHeight = 10;
    }
}
