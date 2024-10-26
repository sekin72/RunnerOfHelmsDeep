using UnityEngine;

namespace GameClient.GameData
{
    [CreateAssetMenu(fileName = "PlayerUnitDataHolder", menuName = "GameClient/PlayerUnitData", order = 4)]
    public class PlayerUnitDataHolder : ScriptableObject
    {
        public int PoolKey;
        public int Damage;
        public float Cooldown;
        public int Cost;
        public float CostMultiplier = 1.2f;

        public float Range = 5f;
        public float DeadRange = 0f;
    }
}
