using System;
using System.Threading;
using CerberusFramework.Core.MVC;
using CerberusFramework.Utilities.MonoBehaviourUtilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameClient.Runtime.Constants;
using UnityEngine;

namespace GameClient.Runtime.PlayerUnits
{
    [RequireComponent(typeof(CollisionDetector))]
    public class PlayerUnitView : View
    {
        public event Action<EnemyView> EnemyEnteredReach;
        public event Action<EnemyView> EnemyExitedReach;

        [SerializeField] private Transform _body;
        [SerializeField] protected Animator Animator;
        [SerializeField] private CollisionDetector _collisionDetector;
        [SerializeField] private SphereCollider _sphereCollider;

        private Tween _delayTween;
        private Tween _rotateTween;
        private bool _isFiring;

        private PlayerUnitData _playerUnitData;

        public override void Initialize()
        {
            _playerUnitData = (PlayerUnitData)Data;
            transform.position = _playerUnitData.Position;
            var playerUnitDataHolder = _playerUnitData.PlayerUnitDataHolder;

            _isFiring = false;
            _sphereCollider.radius = playerUnitDataHolder.Range;

            transform.localScale = Vector3.one * 2.5f;
        }

        public override void Dispose()
        {
        }

        public override void Activate()
        {
            transform.LookAt(_playerUnitData.LookAtTile.Position);

            _collisionDetector.TriggerEntered += OnTriggerEntered;
            _collisionDetector.TriggerExited += OnTriggerExited;
        }

        public override void Deactivate()
        {
            _delayTween?.Kill();
            _rotateTween?.Kill();
            _collisionDetector.TriggerEntered -= OnTriggerEntered;
            _collisionDetector.TriggerExited -= OnTriggerExited;
        }

        private void OnTriggerEntered(Collider other)
        {
            if (other.TryGetComponent(out EnemyView enemy))
            {
                EnemyEnteredReach?.Invoke(enemy);
            }
        }

        private void OnTriggerExited(Collider other)
        {
            if (other.TryGetComponent(out EnemyView enemy))
            {
                EnemyExitedReach?.Invoke(enemy);
            }
        }

        public virtual async UniTask SendProjectile(Enemy enemy, GameObject particle, CancellationToken cancellationToken)
        {
            particle.transform.position = transform.position;
            particle.transform.position += Vector3.up * 1.5f;

            Animator.SetTrigger(AnimationConstants.Fire);
            await particle.GetComponent<ProjectileMover>().Move(enemy.View.transform.position + (Vector3.up * 1.5f), cancellationToken);
        }

        public virtual UniTask WaitProjectileFinish(GameObject particle, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public void TurnToClosestEnemy(EnemyView enemyView)
        {
            _rotateTween?.Kill();
            _rotateTween = _body.DOLookAt(enemyView.transform.position, 0.1f);
        }
    }
}
