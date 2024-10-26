using UnityEngine;

namespace CFGameClient.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "CerberusFramework/Data/GameSettings", order = 3)]
    public class GameSettings : ScriptableObject
    {
        public int StartingCoin = 200;
    }
}
