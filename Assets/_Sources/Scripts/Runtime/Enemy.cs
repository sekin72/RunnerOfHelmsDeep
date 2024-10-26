using System.Threading;
using CerberusFramework.Core.MVC;
using Cysharp.Threading.Tasks;
using GameClient.Runtime.Events;
using MessagePipe;

namespace GameClient.Runtime
{
    public class Enemy : Controller<EnemyView, EnemyData>
    {
        public bool IsAlive => !Data.Deactivated;

        private IPublisher<EnemyDisposedEvent> _enemyDisposedEventPublisher;
        private IPublisher<EnemyKilledEvent> _enemyKilledEventPublisher;
        private IPublisher<EnemyReachedMainTowerEvent> _enemyReachedMainTowerEventPublisher;

        private int _health;

        public override void Initialize(CancellationToken cancellationToken)
        {
            base.Initialize(cancellationToken);

            View.ReachedMainTower += OnReachedMainTower;
            _health = Data.EnemyDataHolder.Health;
            _enemyKilledEventPublisher = GlobalMessagePipe.GetPublisher<EnemyKilledEvent>();
            _enemyDisposedEventPublisher = GlobalMessagePipe.GetPublisher<EnemyDisposedEvent>();
            _enemyReachedMainTowerEventPublisher = GlobalMessagePipe.GetPublisher<EnemyReachedMainTowerEvent>();
        }

        public override bool Dispose()
        {
            if (base.Dispose())
            {
                return true;
            }

            _enemyDisposedEventPublisher.Publish(new EnemyDisposedEvent(this));

            return false;
        }

        public override void Activate()
        {
            View.Activate();
        }

        public override bool Deactivate()
        {
            if (base.Deactivate())
            {
                return true;
            }

            View.ReachedMainTower -= OnReachedMainTower;

            return false;
        }

        public void GetDamaged(int damage)
        {
            if (Data.Deactivated)
            {
                return;
            }

            _health -= damage;

            if (_health <= 0)
            {
                Deactivate();
                View.OnEnemyDied().Forget();
                _enemyKilledEventPublisher.Publish(new EnemyKilledEvent(this, Data.EnemyDataHolder.Score, Data.EnemyDataHolder.Gold));
            }
        }

        public void OnReachedMainTower()
        {
            if (Data.Deactivated)
            {
                return;
            }

            _enemyReachedMainTowerEventPublisher.Publish(new EnemyReachedMainTowerEvent(Data.EnemyDataHolder.Damage));
            Deactivate();
        }
    }
}
