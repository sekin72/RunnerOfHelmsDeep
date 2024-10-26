using CerberusFramework.Core.Systems;

namespace GameClient.Runtime.Systems.EnemyControllerSystem
{
    public interface IEnemyControllerSystem : IGameSystem
    {
        Enemy GetEnemyFromView(EnemyView enemyView);
    }
}
