using System;
using CerberusFramework.Managers.Asset;
using CerberusFramework.UI.Components;
using Cysharp.Threading.Tasks;
using GameClient.Runtime.Events;
using GameClient.Runtime.Systems.PlayerUnitControllerSystem;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace GameClient.UI
{
    public class LevelScenePanel : MonoBehaviour
    {
        [SerializeField] private CFText _scoreText;
        [SerializeField] private CFText _goldText;

        [SerializeField] private PlayerUnitSeller _humanArcher;
        [SerializeField] private PlayerUnitSeller _elfArcher;
        [SerializeField] private PlayerUnitSeller _wizard;

        private IDisposable _messageSubscription;

        private AddressableManager _addressableManager;
        private PlayerUnitControllerSystem _playerUnitControllerSystem;

        [Inject]
        public void Inject(AddressableManager addressableManager)
        {
            _addressableManager = addressableManager;
        }

        public async UniTask Initialize(Camera camera)
        {
            var bagBuilder = DisposableBag.CreateBuilder();
            GlobalMessagePipe.GetSubscriber<ScoreChangedEvent>().Subscribe(OnScoreChanged).AddTo(bagBuilder);
            GlobalMessagePipe.GetSubscriber<GoldChangedEvent>().Subscribe(OnGoldChangedEvent).AddTo(bagBuilder);
            _messageSubscription = bagBuilder.Build();

            _playerUnitControllerSystem = await _addressableManager.LoadAssetAsync<PlayerUnitControllerSystem>("PlayerUnitControllerSystem");

            _humanArcher.Initialize(camera, _playerUnitControllerSystem.PlayerUnitData[0].Cost, _playerUnitControllerSystem.PlayerUnitData[0].PoolKey);
            _elfArcher.Initialize(camera, _playerUnitControllerSystem.PlayerUnitData[1].Cost, _playerUnitControllerSystem.PlayerUnitData[1].PoolKey);
            _wizard.Initialize(camera, _playerUnitControllerSystem.PlayerUnitData[2].Cost, _playerUnitControllerSystem.PlayerUnitData[2].PoolKey);
        }

        public void Dispose()
        {
            _messageSubscription?.Dispose();

            _humanArcher.Dispose();
            _elfArcher.Dispose();
            _wizard.Dispose();
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            _scoreText.Text = evt.Score.ToString();
        }

        private void OnGoldChangedEvent(GoldChangedEvent evt)
        {
            _goldText.Text = evt.Gold.ToString();
        }
    }
}
