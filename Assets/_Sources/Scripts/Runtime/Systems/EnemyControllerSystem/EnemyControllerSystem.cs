using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CerberusFramework.Core;
using CerberusFramework.Managers.Asset;
using CerberusFramework.Utilities.Extensions;
using CFGameClient;
using CFGameClient.Core;
using CFGameClient.Core.Systems.ViewSpawnerSystem;
using Cysharp.Threading.Tasks;
using GameClient.GameData;
using GameClient.Runtime.Events;
using GameClient.Runtime.Systems.EnvironmentCreatorSystem;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace GameClient.Runtime.Systems.EnemyControllerSystem
{
    [CreateAssetMenu(fileName = "EnemyControllerSystem", menuName = "GameClient/Systems/EnemyControllerSystem", order = 3)]
    public class EnemyControllerSystem : GameSystem, IEnemyControllerSystem
    {
        public override Type RegisterType => typeof(IEnemyControllerSystem);

        public List<EnemyDataHolder> EnemyData;

        private IDisposable _messageSubscription;
        private IViewSpawnerSystem _viewSpawnerSystem;
        private IObjectResolver _objectResolver;

        private List<Enemy> _enemies = new();

        private AddressableManager _addressableManager;
        private DifficultyData _difficultyData;
        private WaveData _currentWaveData;
        private int _currentDifficulty;

        private CancellationTokenSource _linkedCancellationTokenSource;

        private IEnvironmentCreatorSystem _environmentCreatorSystem;

        [Inject]
        public void Inject(IObjectResolver objectResolver, AddressableManager addressableManager)
        {
            _objectResolver = objectResolver;
            _addressableManager = addressableManager;
        }

        public override async UniTask Initialize(GameSessionBase gameSession, CancellationToken cancellationToken)
        {
            _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await base.Initialize(gameSession, cancellationToken);

            Session = (GameSession)gameSession;
            _enemies = new();

            _environmentCreatorSystem = Session.GetSystem<IEnvironmentCreatorSystem>();
            _viewSpawnerSystem = Session.GetSystem<IViewSpawnerSystem>();

            _difficultyData = await _addressableManager.LoadAssetAsync<DifficultyData>("DifficultyData", cancellationToken);

            _currentDifficulty = Session.GameSessionSaveStorage.Difficulty;
            _currentWaveData = _difficultyData.WaveDatas[_currentDifficulty];

            var bagBuilder = DisposableBag.CreateBuilder();
            GlobalMessagePipe.GetSubscriber<ScoreChangedEvent>().Subscribe(OnScoreChanged).AddTo(bagBuilder);
            GlobalMessagePipe.GetSubscriber<EnemyKilledEvent>().Subscribe(OnEnemyKilled).AddTo(bagBuilder);
            GlobalMessagePipe.GetSubscriber<EnemyDisposedEvent>().Subscribe(OnEnemyDisposed).AddTo(bagBuilder);
            GlobalMessagePipe.GetSubscriber<FirstInputTakenEvent>().Subscribe(OnFirstInputTakenEvent).AddTo(bagBuilder);
            _messageSubscription = bagBuilder.Build();
        }

        public override void Activate()
        {
        }

        public override void Deactivate()
        {
            foreach (var enemy in _enemies)
            {
                enemy.Deactivate();
            }
        }

        public override void Dispose()
        {
            for (var i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Dispose();
            }

            _messageSubscription?.Dispose();
        }

        private void OnFirstInputTakenEvent(FirstInputTakenEvent evt)
        {
            LoadEnemies().Forget();
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            var difficulty = 0;

            while (evt.Score >= _difficultyData.WaveDatas[difficulty].Score)
            {
                difficulty++;
            }

            if (difficulty != _currentDifficulty)
            {
                _currentDifficulty = difficulty;
                Session.GameSessionSaveStorage.Difficulty = _currentDifficulty;
                Session.SaveGameSessionStorage();

                _currentWaveData = _difficultyData.WaveDatas[_currentDifficulty];
                _linkedCancellationTokenSource.Cancel();
                _linkedCancellationTokenSource.Dispose();

                _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(Session.CancellationTokenSource.Token);
                LoadEnemies().Forget();
            }
        }

        private async UniTask LoadEnemies()
        {
            var randomKey = _currentWaveData.EnemyPoolKeys.GetRandomElement();
            var enemyDataHolder = EnemyData.FirstOrDefault(x => x.PoolKey == randomKey);

            var enemyData = new EnemyData(enemyDataHolder, _environmentCreatorSystem.Path);
            var enemyView = _viewSpawnerSystem.Spawn<EnemyView>(GamePoolKeys.FromId(enemyDataHolder.PoolKey));
            var enemy = new Enemy();
            _objectResolver.Inject(enemy);
            enemy.SetDataAndView(enemyData, enemyView);
            enemy.Initialize(_linkedCancellationTokenSource.Token);

            _enemies.Add(enemy);
            enemy.Activate();

            await UniTask.Delay(TimeSpan.FromSeconds(_currentWaveData.TimeInterval), cancellationToken: _linkedCancellationTokenSource.Token);

            LoadEnemies().Forget();
        }

        private void OnEnemyDisposed(EnemyDisposedEvent enemyDiedEvent)
        {
            var enemy = enemyDiedEvent.Enemy;
            _enemies.Remove(enemy);
            _viewSpawnerSystem.Despawn(GamePoolKeys.FromId(enemy.Data.EnemyDataHolder.PoolKey), enemy.View);
        }

        private void OnEnemyKilled(EnemyKilledEvent enemyKilledEvent)
        {
            var enemy = enemyKilledEvent.Enemy;
            enemy.Dispose();
        }

        public Enemy GetEnemyFromView(EnemyView enemyView)
        {
            return _enemies.FirstOrDefault(x => x.View == enemyView);
        }
    }
}
