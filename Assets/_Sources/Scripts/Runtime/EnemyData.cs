using System.Collections.Generic;
using CerberusFramework.Core.MVC;
using GameClient.GameData;

namespace GameClient.Runtime
{
    public class EnemyData : Data
    {
        public EnemyDataHolder EnemyDataHolder { get; private set; }
        public List<Tile> Path { get; private set; }

        public EnemyData(EnemyDataHolder enemyDataHolder, List<Tile> path)
        {
            EnemyDataHolder = enemyDataHolder;
            Path = path;
        }
    }
}
