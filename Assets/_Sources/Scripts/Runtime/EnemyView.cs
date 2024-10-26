using System;
using System.Threading;
using CerberusFramework.Core.MVC;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameClient.Runtime
{
    public class EnemyView : View
    {
        public event Action ReachedMainTower;

        [SerializeField] private GameObject _bodyObject;
        [SerializeField] private ParticleSystem _deathParticle;

        private Tween _moveTween;
        private Tween _rotateTween;

        private int _currentPathIndex;

        private CancellationTokenSource _cancellationTokenSource;

        private EnemyData _enemyData;

        public override void Initialize()
        {
            _enemyData = (EnemyData)Data;

            _bodyObject.SetActive(true);
            _deathParticle?.gameObject.SetActive(false);

            _cancellationTokenSource = new CancellationTokenSource();

            transform.localScale = Vector3.one * 2.5f;
        }

        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public override void Activate()
        {
            _currentPathIndex = 0;

            transform.position = _enemyData.Path[_currentPathIndex].Position;
            transform.LookAt(_enemyData.Path[1].Position);
            Move();
        }

        public override void Deactivate()
        {
            _moveTween?.Kill();
            _rotateTween?.Kill();
        }

        public async UniTask OnEnemyDied()
        {
            _bodyObject.SetActive(false);
            _deathParticle?.gameObject.SetActive(true);
            _deathParticle?.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(_deathParticle.main.duration), cancellationToken: _cancellationTokenSource.Token);

            _deathParticle?.gameObject.SetActive(false);
        }

        private void Move()
        {
            if (_currentPathIndex < _enemyData.Path.Count - 1)
            {
                _moveTween = transform.DOMove(_enemyData.Path[++_currentPathIndex].Position, _enemyData.EnemyDataHolder.Speed).OnComplete(Move);
                _rotateTween = transform.DOLookAt(_enemyData.Path[_currentPathIndex].Position, 0.2f);
            }
            else
            {
                ReachedMainTower?.Invoke();
            }
        }
    }
}
