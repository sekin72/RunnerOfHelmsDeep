using System;
using System.Collections.Generic;
using System.Threading;
using CerberusFramework.Core;
using CerberusFramework.Managers.Asset;
using CerberusFramework.Managers.Pool;
using CFGameClient.Core;
using Cysharp.Threading.Tasks;
using GameClient.GameData;
using UnityEngine;
using VContainer;

namespace GameClient.Runtime.Systems.EnvironmentCreatorSystem
{
    [CreateAssetMenu(fileName = "EnvironmentCreatorSystem", menuName = "GameClient/Systems/EnvironmentCreatorSystem", order = 1)]
    public sealed class EnvironmentCreatorSystem : GameSystem, IEnvironmentCreatorSystem
    {
        public override Type RegisterType => typeof(IEnvironmentCreatorSystem);

        private EnvironmentCreatorSystemView _view;

        private PoolManager _poolManager;
        private AddressableManager _addressableManager;

        public List<Tile> Path { get; private set; }

        [Inject]
        private void Inject(PoolManager poolManager, AddressableManager addressableManager)
        {
            _poolManager = poolManager;
            _addressableManager = addressableManager;
        }

        public override async UniTask Initialize(GameSessionBase gameSessionBase, CancellationToken cancellationToken)
        {
            await base.Initialize(gameSessionBase, cancellationToken);

            var environmentData = await _addressableManager.LoadAssetAsync<EnvironmentData>("EnvironmentData", cancellationToken);

            _view = _poolManager.GetGameObject(CFPoolKeys.FromIdOrName(nameof(EnvironmentCreatorSystemView))).GetComponent<EnvironmentCreatorSystemView>();
            _view.Initialize(environmentData);

            UnityEngine.Random.InitState(Session.GameSessionSaveStorage.LevelRandomSeed);
            _view.CreateGroundTiles();

            Path = await _view.CreatePath(cancellationToken);

            _view.CreateEnemySpawnPoint(Path[0].Index.x, Path[0].Index.y);
            _view.CreateMainTower(Path[^1].Index.x, Path[^1].Index.y);
            _view.CreatePlayerUnitTiles(Path);
        }

        public override void Activate()
        {
        }

        public override void Deactivate()
        {
        }

        public override void Dispose()
        {
            _view.Dispose();
            _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(nameof(EnvironmentCreatorSystemView)), _view.gameObject);
        }

        public Tile GetTile(Vector2Int index)
        {
            return _view.GetTile(index);
        }

        public GameObject GetMainTower()
        {
            return _view.GetMainTower();
        }
    }
}
