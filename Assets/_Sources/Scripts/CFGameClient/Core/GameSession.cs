using System;
using System.Collections.Generic;
using System.Threading;
using CerberusFramework.Core;
using CerberusFramework.Core.Scenes;
using CerberusFramework.Core.Systems;
using CerberusFramework.Managers.Data;
using CerberusFramework.Managers.Loading;
using CerberusFramework.Managers.Pool;
using CerberusFramework.Managers.Sound;
using CerberusFramework.Managers.UI;
using CerberusFramework.Managers.Vibration;
using CerberusFramework.UI.Popups;
using CerberusFramework.Utilities;
using CFGameClient.Core.Scenes;
using CFGameClient.Data;
using CFGameClient.Managers.Data;
using CFGameClient.UI.Popups.WinPopupVariant;
using Cysharp.Threading.Tasks;
using GameClient.Runtime.Events;
using GameClient.Runtime.Systems.EnvironmentCreatorSystem;
using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace CFGameClient.Core
{
    public class GameSession : GameSessionBase
    {
        private readonly IObjectResolver _resolver;
        private IDisposable _messageSubscription;

        private List<ITickable> _tickables;
        private List<ILateTickable> _lateTickables;

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public LockBin InputDisabled { get; private set; }

        private readonly ProjectDataManager _projectDataManager;
        private readonly PopupManager _popupManager;
        private readonly SoundManager _soundManager;
        private readonly VibrationManager _vibrationManager;
        private readonly AssetManager _assetManager;

        private List<IGameSystem> _gameSystems;
        private Dictionary<Type, IGameSystem> _gameSystemsDictionary;

        private LevelSceneController _levelSceneController;

        public GameSessionSaveStorage GameSessionSaveStorage { get; private set; }
        public GameSettings GameSettings { get; private set; }

        private bool _deactivated;
        private bool _disposed;

        [Inject]
        public GameSession(
            IObjectResolver resolver,
            ProjectDataManager projectDataManager,
            LoadingManager loadingManager,
            PopupManager popupManager,
            SoundManager soundManager,
            VibrationManager vibrationManager,
            AssetManager assetManager)
        {
            _resolver = resolver;
            _projectDataManager = projectDataManager;
            _popupManager = popupManager;
            _soundManager = soundManager;
            _vibrationManager = vibrationManager;
            _assetManager = assetManager;
        }

        public override async UniTask Initialize(SceneController levelSceneController)
        {
            _disposed = false;
            _deactivated = false;

            _levelSceneController = (LevelSceneController)levelSceneController;

            Application.targetFrameRate = 60;

            CancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;

            _soundManager.StopAll();
            _soundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.BattleBG.Id), playInLoop: true, volumeMultiplier: 0.5f);
            _soundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.BattleSwords.Id), playInLoop: true, volumeMultiplier: 0.5f);

            _gameSystems = ListPool<IGameSystem>.Get();
            _gameSystemsDictionary = DictionaryPool<Type, IGameSystem>.Get();

            var bagBuilder = DisposableBag.CreateBuilder();
            GlobalMessagePipe.GetSubscriber<EnemyReachedMainTowerEvent>().Subscribe(OnEnemyReachedTowerEvent).AddTo(bagBuilder);
            _messageSubscription = bagBuilder.Build();

            InputDisabled = new LockBin();
            _tickables = ListPool<ITickable>.Get();
            _lateTickables = ListPool<ILateTickable>.Get();

            GameSessionSaveStorage = _projectDataManager.ProjectSaveStorage.GameSessionSaveStorage;
            GameSettings = _projectDataManager.GameSettings;

            var tasks = new List<UniTask>();

            var systemsCollection = _assetManager.GetScriptableAsset<SystemsCollection>(CFPoolKeys.SystemsCollection);

            if (systemsCollection == null)
            {
                tasks.Add(_assetManager.GetScriptableAsset<SystemsCollection>(CFPoolKeys.SystemsCollection, cancellationToken)
                .ContinueWith((col) => systemsCollection = col));
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }

            RegisterSystems(systemsCollection);

            foreach (var system in _gameSystems)
            {
                await system.Initialize(this, cancellationToken);
            }

            _levelSceneController.RTSCamera.SetTarget(GetSystem<IEnvironmentCreatorSystem>().GetMainTower().transform);

            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);

            _levelSceneController.RTSCamera.ResetTarget();
        }

        public void Activate()
        {
            foreach (var system in _gameSystems)
            {
                system.Activate();
            }

            RegisterTicks();

            _levelSceneController.RTSCamera.Initialize();
        }

        private void Deactivate()
        {
            if (_deactivated)
            {
                return;
            }

            _deactivated = true;

            _levelSceneController.RTSCamera.Dispose();

            if (_tickables.Count > 0)
            {
                _levelSceneController.Tick -= Tick;
            }

            if (_lateTickables.Count > 0)
            {
                _levelSceneController.LateTick -= LateTick;
            }

            ListPool<ITickable>.Release(_tickables);
            ListPool<ILateTickable>.Release(_lateTickables);

            for (var i = _gameSystems.Count - 1; i >= 0; i--)
            {
                _gameSystems[i]?.Deactivate();
            }
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Deactivate();

            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;

            _disposed = true;

            _messageSubscription?.Dispose();

            for (var i = _gameSystems.Count - 1; i >= 0; i--)
            {
                _gameSystems[i]?.Dispose();
            }

            ListPool<IGameSystem>.Release(_gameSystems);
            DictionaryPool<Type, IGameSystem>.Release(_gameSystemsDictionary);
        }

        private void Tick()
        {
            for (var i = 0; i < _tickables.Count; i++)
            {
                _tickables[i].Tick();
            }
        }

        private void LateTick()
        {
            for (var i = 0; i < _lateTickables.Count; i++)
            {
                _lateTickables[i].LateTick();
            }
        }

        private void OnEnemyReachedTowerEvent(EnemyReachedMainTowerEvent e)
        {
            LevelFinished();
        }

        public void LevelFinished()
        {
            if (_deactivated)
            {
                return;
            }

            Deactivate();

            GameSessionSaveStorage.GameplayFinished = true;
            SaveGameSessionStorage();

            _soundManager.StopSound(CFSoundTypes.FromId(GameSoundKeys.BattleSwords.Id), false, 0.25f);
            _soundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.Victory.Id));

            //_soundManager.PlayOneShot(SoundTypes.LevelCompleted);
            _vibrationManager.Vibrate(VibrationType.Success);
            _popupManager.Open<WinPopupVariant, WinPopupVariantView, WinPopupVariantData>(
                new WinPopupVariantData(
                _levelSceneController,
                GameSessionSaveStorage),
                    PopupShowActions.CloseAll,
                CancellationTokenSource.Token).Forget();
        }

        private void RegisterTicks()
        {
            if (_tickables.Count > 0)
            {
                _levelSceneController.Tick += Tick;
            }

            if (_lateTickables.Count > 0)
            {
                _levelSceneController.LateTick += LateTick;
            }
        }

        private void RegisterSystems(SystemsCollection systemsCollection)
        {
            foreach (var system in systemsCollection.Systems)
            {
                _gameSystems.Add(system);
                _resolver.Inject(system);
                _gameSystemsDictionary.Add(system.RegisterType, system);

                if (system is ITickable tickable)
                {
                    _tickables.Add(tickable);
                }

                if (system is ILateTickable lateTickable)
                {
                    _lateTickables.Add(lateTickable);
                }
            }
        }

        public override T GetSystem<T>()
        {
            _gameSystemsDictionary.TryGetValue(typeof(T), out var system);
            return (T)system;
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
        }

        public void SaveGameSessionStorage()
        {
            _projectDataManager.ProjectSaveStorage.GameSessionSaveStorage = GameSessionSaveStorage;
            _projectDataManager.Save();
        }
    }
}
