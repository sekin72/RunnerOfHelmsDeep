using UnityEngine;

namespace GameClient.GameData
{
    [CreateAssetMenu(fileName = "EnemyDataHolder", menuName = "GameClient/EnemyData", order = 3)]
    public class EnemyDataHolder : ScriptableObject
    {
        public int PoolKey;
        public int Health;
        public int Damage;
        public float Speed;
        public int Score;
        public int Gold;
    }
}
