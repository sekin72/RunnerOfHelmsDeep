using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CerberusFramework.Core;
using CerberusFramework.Managers.Pool;
using CFGameClient;
using CFGameClient.Core;
using CFGameClient.Core.Systems.ViewSpawnerSystem;
using Cysharp.Threading.Tasks;
using GameClient.GameData;
using GameClient.Runtime.Events;
using GameClient.Runtime.PlayerUnits;
using GameClient.Runtime.Systems.EnvironmentCreatorSystem;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameClient.Runtime.Systems.PlayerUnitControllerSystem
{
    [CreateAssetMenu(fileName = "PlayerUnitControllerSystem", menuName = "GameClient/Systems/PlayerUnitControllerSystem", order = 3)]
    public class PlayerUnitControllerSystem : GameSystem, IPlayerUnitControllerSystem, ILateTickable
    {
        public override Type RegisterType => typeof(IPlayerUnitControllerSystem);

        public List<PlayerUnitDataHolder> PlayerUnitData;

        private IViewSpawnerSystem _viewSpawnerSystem;
        private IObjectResolver _objectResolver;
        private IEnvironmentCreatorSystem _environmentCreatorSystem;

        private List<PlayerUnit> _playerUnits = new();

        private IDisposable _messageSubscription;
        private IPublisher<PlayerUnitSoldEvent> _playerUnitSoldEventPublisher;
        private IPublisher<FirstInputTakenEvent> _firstInputTakenEventPublisher;
        private IPublisher<PlayerUnitCostChangedEvent> _playerUnitCostChangedEventPublisher;

        private bool _firstInputTaken;

        private Dictionary<int, int> _playerUnitCostDictionary = new();

        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        public void Inject(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public override async UniTask Initialize(GameSessionBase gameSessionBase, CancellationToken cancellationToken)
        {
            await base.Initialize(gameSessionBase, cancellationToken);

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _firstInputTaken = false;

            var bagBuilder = DisposableBag.CreateBuilder();
            GlobalMessagePipe.GetSubscriber<SellerPlacedEvent>().Subscribe(OnSellerPlacedEvent).AddTo(bagBuilder);
            _messageSubscription = bagBuilder.Build();

            _playerUnitSoldEventPublisher = GlobalMessagePipe.GetPublisher<PlayerUnitSoldEvent>();
            _firstInputTakenEventPublisher = GlobalMessagePipe.GetPublisher<FirstInputTakenEvent>();
            _playerUnitCostChangedEventPublisher = GlobalMessagePipe.GetPublisher<PlayerUnitCostChangedEvent>();

            _playerUnits = new();
            _playerUnitCostDictionary = new();

            for (var i = 0; i < PlayerUnitData.Count; i++)
            {
                _playerUnitCostDictionary.Add(PlayerUnitData[i].PoolKey, PlayerUnitData[i].Cost);
            }

            _environmentCreatorSystem = Session.GetSystem<IEnvironmentCreatorSystem>();
            _viewSpawnerSystem = Session.GetSystem<IViewSpawnerSystem>();

            var placedPlayerUnitPositions = Session.GameSessionSaveStorage.PlacedPlayerUnitPositions;
            var placedPlayerUnitPoolKeys = Session.GameSessionSaveStorage.PlacedPlayerUnitPoolKeys;

            for (var i = 0; i < placedPlayerUnitPositions.Count; i++)
            {
                var index = placedPlayerUnitPositions[i];
                var poolKeyID = placedPlayerUnitPoolKeys[i];
                var tile = _environmentCreatorSystem.GetTile(index);
                var playerUnitData = PlayerUnitData.FirstOrDefault(playerUnit => playerUnit.PoolKey == poolKeyID);

                CreatePlayerUnit(playerUnitData, tile);
            }
        }

        public override void Activate()
        {
            if (!_firstInputTaken && Session.GameSessionSaveStorage.PlacedPlayerUnitPositions.Count > 0)
            {
                _firstInputTakenEventPublisher.Publish(new FirstInputTakenEvent());
                _firstInputTaken = true;
            }

            for (var i = 0; i < PlayerUnitData.Count; i++)
            {
                _playerUnitCostChangedEventPublisher.Publish(new PlayerUnitCostChangedEvent(PlayerUnitData[i].PoolKey, _playerUnitCostDictionary[PlayerUnitData[i].PoolKey]));
            }
        }

        public override void Deactivate()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            foreach (var playerUnit in _playerUnits)
            {
                playerUnit.Deactivate();
            }
        }

        public override void Dispose()
        {
            _messageSubscription?.Dispose();
            foreach (var playerUnit in _playerUnits)
            {
                playerUnit.Dispose();
                _viewSpawnerSystem.Despawn(GamePoolKeys.FromId(playerUnit.Data.PlayerUnitDataHolder.PoolKey), playerUnit.View);
            }
        }

        private void OnSellerPlacedEvent(SellerPlacedEvent evt)
        {
            var poolKey = CFPoolKeys.FromIdOrName(evt.PoolKey.ToString());
            var cost = _playerUnitCostDictionary[poolKey.Id];
            var playerUnitData = PlayerUnitData.FirstOrDefault(playerUnit => playerUnit.PoolKey == poolKey.Id);

            var passiveTile = evt.PassiveTile;

            if (_playerUnits.FirstOrDefault(playerUnit => playerUnit.Data.AttachedTile.Index == passiveTile.AttachedTile.Index) != null)
            {
                return;
            }

            CreatePlayerUnit(playerUnitData, passiveTile.AttachedTile);
            _playerUnitSoldEventPublisher.Publish(new PlayerUnitSoldEvent(-cost));

            Session.GameSessionSaveStorage.PlacedPlayerUnitPoolKeys.Add(playerUnitData.PoolKey);
            Session.GameSessionSaveStorage.PlacedPlayerUnitPositions.Add(passiveTile.AttachedTile.Index);

            Session.SaveGameSessionStorage();

            if (!_firstInputTaken)
            {
                _firstInputTakenEventPublisher.Publish(new FirstInputTakenEvent());
                _firstInputTaken = true;
            }
        }

        private void CreatePlayerUnit(PlayerUnitDataHolder playerUnitDataHolder, Tile tile)
        {
            var playerUnitData = new PlayerUnitData(playerUnitDataHolder, tile, tile.Position, _environmentCreatorSystem.Path[0]);
            var playerUnitView = _viewSpawnerSystem.Spawn<PlayerUnitView>(GamePoolKeys.FromId(playerUnitDataHolder.PoolKey));
            var playerUnit = new PlayerUnit();
            if (playerUnitDataHolder.PoolKey == GamePoolKeys.Wizard.Id)
            {
                playerUnit = new Wizard();
            }

            _objectResolver.Inject(playerUnit);
            playerUnit.SetDataAndView(playerUnitData, playerUnitView);
            playerUnit.Initialize(_cancellationTokenSource.Token);

            _playerUnits.Add(playerUnit);
            playerUnit.Activate();

            _playerUnitCostDictionary[playerUnitDataHolder.PoolKey] = (int)(_playerUnitCostDictionary[playerUnitDataHolder.PoolKey] * playerUnitDataHolder.CostMultiplier);
            _playerUnitCostChangedEventPublisher.Publish(new PlayerUnitCostChangedEvent(playerUnitDataHolder.PoolKey, _playerUnitCostDictionary[playerUnitDataHolder.PoolKey]));
        }

        public void LateTick()
        {
            foreach (var playerUnit in _playerUnits)
            {
                playerUnit.LateTick();
            }
        }
    }
}
