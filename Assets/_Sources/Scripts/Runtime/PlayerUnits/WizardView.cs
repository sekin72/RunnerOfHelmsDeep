using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameClient.Runtime.Constants;
using UnityEngine;

namespace GameClient.Runtime.PlayerUnits
{
    public class WizardView : PlayerUnitView
    {
        public override async UniTask SendProjectile(Enemy enemy, GameObject particle, CancellationToken cancellationToken)
        {
            particle.transform.position = enemy.View.transform.position;
            var particleSystem = particle.GetComponent<ParticleSystem>();

            Animator.SetTrigger(AnimationConstants.Fire);
            await UniTask.Delay(TimeSpan.FromSeconds(particleSystem.main.duration / 4f), cancellationToken: cancellationToken);

            particleSystem.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(particleSystem.main.duration / 4f), cancellationToken: cancellationToken);
        }

        public override async UniTask WaitProjectileFinish(GameObject particle, CancellationToken cancellationToken)
        {
            var particleSystem = particle.GetComponent<ParticleSystem>();
            await UniTask.Delay(TimeSpan.FromSeconds(particleSystem.main.duration * 3f / 4f), cancellationToken: cancellationToken);
        }
    }
}
