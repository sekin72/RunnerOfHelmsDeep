using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CerberusFramework.Core.MVC;
using CerberusFramework.Managers.Pool;
using CerberusFramework.Managers.Sound;
using CFGameClient;
using CFGameClient.Core;
using CFGameClient.Core.Events;
using Cysharp.Threading.Tasks;
using GameClient.Runtime.Systems.EnemyControllerSystem;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameClient.Runtime.PlayerUnits
{
    public class PlayerUnit : Controller<PlayerUnitView, PlayerUnitData>, ILateTickable
    {
        protected GameSession Session;
        private readonly List<Enemy> _enemyList = new();

        protected PoolManager PoolManager;
        protected SoundManager SoundManager;
        protected IEnemyControllerSystem EnemyControllerSystem;

        protected IPublisher<AttachParticleEvent> AttachParticleEventPublisher;
        protected IPublisher<DetachParticleEvent> DetachParticleEventPublisher;

        protected Enemy ClosestEnemy;

        [Inject]
        public void Inject(PoolManager poolManager, SoundManager soundManager, GameSession gameSession)
        {
            PoolManager = poolManager;
            SoundManager = soundManager;
            Session = gameSession;
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
            base.Initialize(cancellationToken);

            EnemyControllerSystem = Session.GetSystem<IEnemyControllerSystem>();

            View.EnemyEnteredReach += View_EnemyEnteredReach;
            View.EnemyExitedReach += View_EnemyExitedReach;

            ClosestEnemy = null;

            AttachParticleEventPublisher = GlobalMessagePipe.GetPublisher<AttachParticleEvent>();
            DetachParticleEventPublisher = GlobalMessagePipe.GetPublisher<DetachParticleEvent>();
        }

        public override void Activate()
        {
            View.Activate();

            Attack().Forget();
        }

        public override bool Deactivate()
        {
            if (base.Deactivate())
            {
                return true;
            }

            View.EnemyEnteredReach -= View_EnemyEnteredReach;
            View.EnemyExitedReach -= View_EnemyExitedReach;

            return false;
        }

        public void LateTick()
        {
            if (Data == null || Data.Deactivated || Data.Disposed)
            {
                return;
            }

            for (var i = 0; i < _enemyList.Count; i++)
            {
                if (!_enemyList[i].IsAlive)
                {
                    _enemyList.RemoveAt(i);
                    i--;
                }
            }

            if (_enemyList.Count <= 0)
            {
                ClosestEnemy = null;
                return;
            }

            var enemy = _enemyList.FirstOrDefault(x => x.IsAlive);

            var curDistance = Vector3.Distance(View.transform.position, enemy.View.transform.position);
            for (var i = 0; i < _enemyList.Count; i++)
            {
                if (!_enemyList[i].IsAlive)
                {
                    continue;
                }

                var distance = Vector3.Distance(View.transform.position, _enemyList[i].View.transform.position);
                if (distance < curDistance && distance >= Data.PlayerUnitDataHolder.DeadRange)
                {
                    enemy = _enemyList[i];
                    curDistance = distance;
                }
            }

            if (ClosestEnemy != enemy)
            {
                ClosestEnemy = enemy;
            }

            View.TurnToClosestEnemy(ClosestEnemy.View);
        }

        private void View_EnemyEnteredReach(EnemyView enemyView)
        {
            _enemyList.Add(EnemyControllerSystem.GetEnemyFromView(enemyView));
        }

        private void View_EnemyExitedReach(EnemyView enemyView)
        {
            _enemyList.Remove(EnemyControllerSystem.GetEnemyFromView(enemyView));
        }

        protected virtual async UniTask Attack()
        {
            if (ClosestEnemy == null)
            {
                await UniTask.WaitUntil(() => ClosestEnemy != null, cancellationToken: Session.CancellationTokenSource.Token);
            }

            var particle = PoolManager.GetGameObject(CFPoolKeys.FromIdOrName(nameof(GamePoolKeys.Arrow)));
            AttachParticleEventPublisher.Publish(new AttachParticleEvent(CFPoolKeys.FromIdOrName(GamePoolKeys.Arrow.Name), particle));
            SoundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.Arrow.Id));

            await View.SendProjectile(ClosestEnemy, particle, Session.CancellationTokenSource.Token);

            ClosestEnemy.GetDamaged(Data.PlayerUnitDataHolder.Damage);

            await View.WaitProjectileFinish(particle, Session.CancellationTokenSource.Token);

            DetachParticleEventPublisher.Publish(new DetachParticleEvent(CFPoolKeys.FromIdOrName(GamePoolKeys.Arrow.Name), particle));

            await UniTask.Delay(TimeSpan.FromSeconds(Data.PlayerUnitDataHolder.Cooldown), cancellationToken: Session.CancellationTokenSource.Token);

            Attack().Forget();
        }
    }
}
