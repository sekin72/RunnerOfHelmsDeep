using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameClient.Runtime.PlayerUnits
{
    public class ProjectileMover : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private float _speed = 0.1f;

        private Tween _moveTween;

        public async UniTask Move(Vector3 target, CancellationToken cancellationToken)
        {
            _particleSystem.Play();
            _moveTween = transform.DOMove(target, _speed).SetEase(Ease.Linear);
            await _moveTween.ToUniTask(TweenCancelBehaviour.KillWithCompleteCallback, cancellationToken: cancellationToken);
            _particleSystem.Stop();
        }
    }
}
