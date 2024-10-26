using System;
using System.Threading;
using CerberusFramework.Core.Scenes;
using CerberusFramework.Managers.Loading;
using CerberusFramework.Managers.Sound;
using CerberusFramework.Managers.UI;
using CerberusFramework.UI.Components;
using CerberusFramework.UI.Popups;
using CerberusFramework.UI.Popups.SettingsPopup;
using CFGameClient.Managers.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace CFGameClient.Core.Scenes
{
    public class MainSceneController : SceneController
    {
        protected PopupManager PopupManager;
        private LoadingManager _loadingManager;
        private SoundManager _soundManager;
        private ProjectDataManager _projectDataManager;

        [SerializeField] private CFButton _newGameButton;
        [SerializeField] private CFButton _loadButton;
        [SerializeField] private CFButton _settingsButton;
        [SerializeField] private CFButton _cfDemoButton;

        [Inject]
        public void Inject(
            PopupManager popupManager,
            LoadingManager loadingManager,
            SoundManager soundManager,
            ProjectDataManager projectDataManager)
        {
            PopupManager = popupManager;
            _loadingManager = loadingManager;
            _soundManager = soundManager;
            _projectDataManager = projectDataManager;
        }

        public override async UniTask Activate(CancellationToken cancellationToken)
        {
            await base.Activate(cancellationToken);

            _loadButton.interactable = _projectDataManager.ProjectSaveStorage.GameSessionSaveStorage is { GameplayFinished: false };

            _newGameButton.onClick.AddListener(OnNewGameButtonClick);
            _loadButton.onClick.AddListener(OnLoadButtonClick);
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);
            _cfDemoButton.onClick.AddListener(OnCFDemoButtonClick);
        }

        public override UniTask Deactivate(CancellationToken cancellationToken)
        {
            _soundManager.StopAll();

            _newGameButton.onClick.RemoveListener(OnNewGameButtonClick);
            _loadButton.onClick.RemoveListener(OnLoadButtonClick);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            _cfDemoButton.onClick.RemoveListener(OnCFDemoButtonClick);

            return base.Deactivate(cancellationToken);
        }

        private async void OnNewGameButtonClick()
        {
            var oldHighScore = _projectDataManager.ProjectSaveStorage.GameSessionSaveStorage?.HighScore ?? 0;
            _projectDataManager.ProjectSaveStorage.GameSessionSaveStorage = new GameSessionSaveStorage
            {
                GameplayFinished = false,
                LevelRandomSeed = Mathf.Abs((int)DateTime.Now.Ticks),
                HighScore = oldHighScore,
                Difficulty = 0,
                Gold = _projectDataManager.GameSettings.StartingCoin,
                CurrentScore = 0
            };

            await _loadingManager.LoadLevelScene();
        }

        private async void OnLoadButtonClick()
        {
            await _loadingManager.LoadLevelScene();
        }

        private async void OnCFDemoButtonClick()
        {
            await _loadingManager.LoadCFDemoScene();
        }

        private void OnSettingsButtonClick()
        {
            PopupManager.Open<SettingsPopup, SettingsPopupView, SettingsPopupData>(new SettingsPopupData(), PopupShowActions.CloseAll, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public override void SceneVisible()
        {
            base.SceneVisible();

            _soundManager.StopAll();
            _soundManager.PlayOneShot(CFSoundTypes.FromId(GameSoundKeys.MainTheme.Id), playInLoop: true);
        }
    }
}