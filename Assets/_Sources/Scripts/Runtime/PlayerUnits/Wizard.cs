using System;
using CerberusFramework.Managers.Pool;
using CerberusFramework.Managers.Sound;
using CFGameClient;
using CFGameClient.Core.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameClient.Runtime.PlayerUnits
{
    public class Wizard : PlayerUnit
    {
        protected override async UniTask Attack()
        {
            if (ClosestEnemy == null)
            {
                await UniTask.WaitUntil(() => ClosestEnemy != null, cancellationToken: Session.CancellationTokenSource.Token);
            }

            var particle = PoolManager.GetGameObject(CFPoolKeys.FromIdOrName(nameof(GamePoolKeys.Fireball)));
            AttachParticleEventPublisher.Publish(new AttachParticleEvent(CFPoolKeys.FromIdOrName(GamePoolKeys.Fireball.Name), particle));
            SoundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.Fireball.Id));

            await View.SendProjectile(ClosestEnemy, particle, Session.CancellationTokenSource.Token);

            var sphereCollider = particle.GetComponent<SphereCollider>();
            var colliders = Physics.OverlapSphere(particle.transform.position, sphereCollider.radius);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent(out EnemyView enemyView))
                {
                    var enemy = EnemyControllerSystem.GetEnemyFromView(enemyView);
                    enemy.GetDamaged(Data.PlayerUnitDataHolder.Damage);
                }
            }

            await View.WaitProjectileFinish(particle, Session.CancellationTokenSource.Token);

            DetachParticleEventPublisher.Publish(new DetachParticleEvent(CFPoolKeys.FromIdOrName(GamePoolKeys.Fireball.Name), particle));

            await UniTask.Delay(TimeSpan.FromSeconds(Data.PlayerUnitDataHolder.Cooldown), cancellationToken: Session.CancellationTokenSource.Token);

            Attack().Forget();
        }
    }
}
