using System;
using System.Collections.Generic;
using System.Threading;
using CerberusFramework.Managers.Pool;
using Cysharp.Threading.Tasks;
using GameClient.GameData;
using UnityEngine;
using VContainer;

namespace GameClient.Runtime.Systems.EnvironmentCreatorSystem
{
    public class EnvironmentCreatorSystemView : MonoBehaviour
    {
        private Tile[,] _tileArray;

        private GameObject _tilesParent;

        private readonly List<GameObject> _groundTilesList = new();
        private readonly List<GameObject> _pathsList = new();
        private readonly List<GameObject> _playerUnitTilesList = new();
        private GameObject _mainTower;
        private GameObject _enemySpawnGate;

        private PoolManager _poolManager;

        private EnvironmentData _environmentData;

        private CFPoolKeys _tileKey;
        private int _tileSize;

        private int _totalWidth;
        private int _totalHeight;
        private int _playableWidth;
        private int _playableHeight;

        private int _startingX;
        private int _startingY;

        [Inject]
        private void Inject(PoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public void Initialize(EnvironmentData environmentData)
        {
            _environmentData = environmentData;
            SetParameters();

            for (var i = 0; i < _totalWidth; i++)
            {
                for (var j = 0; j < _totalHeight; j++)
                {
                    _tileArray[i, j] = new Tile
                    {
                        TileObject = _poolManager.GetGameObject(_tileKey),
                        Index = new Vector2Int(i, j),
                        Position = new Vector3(i * _tileSize, 0, j * _tileSize),
                        TileID = 0
                    };

                    _tileArray[i, j].TileObject.name = $"Tile_{i}_{j}";
                    _tileArray[i, j].TileObject.transform.SetParent(_tilesParent.transform);
                    _tileArray[i, j].TileObject.transform.position = _tileArray[i, j].Position;
                }
            }
        }

        private void SetParameters()
        {
            _tilesParent = new GameObject("Grid");

            _tilesParent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.up * 180));

            _totalWidth = _environmentData.TotalWidth;
            _totalHeight = _environmentData.TotalHeight;
            _playableWidth = _environmentData.PlayableWidth;
            _playableHeight = _environmentData.PlayableHeight;
            _startingX = _environmentData.StartingPosition.x;
            _startingY = _environmentData.StartingPosition.y;

            _tileKey = CFPoolKeys.FromIdOrName(_environmentData.TileKey);
            _tileSize = _environmentData.TileSize;

            _tileArray = new Tile[_totalWidth, _totalHeight];
        }

        public void Dispose()
        {
            DisposeGroundTiles();
            DisposePlayerUnitTiles();
            DisposePath();
            DisposeEnemySpawnPoint();
            DisposeMainTower();
        }

        public Tile GetTile(Vector2Int index)
        {
            return _tileArray[index.x, index.y];
        }

        public GameObject GetMainTower()
        {
            return _mainTower;
        }

        public void CreateGroundTiles()
        {
            var poolKey = CFPoolKeys.FromIdOrName(_environmentData.GroundTileKey);

            for (var i = 0; i < _totalWidth; i++)
            {
                for (var j = 0; j < _totalHeight; j++)
                {
                    var tile = _tileArray[i, j];
                    var inside = _poolManager.GetGameObject(poolKey);
                    inside.transform.SetParent(tile.TileObject.transform);
                    inside.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
                    inside.transform.localScale = new Vector3(1, 1, 1);
                    _groundTilesList.Add(inside);
                }
            }
        }

        public void CreateMainTower(int x, int y)
        {
            var poolKey = CFPoolKeys.FromIdOrName(_environmentData.MainTowerKey);

            var tile = _tileArray[x, y];
            _mainTower = _poolManager.GetGameObject(poolKey);
            _mainTower.transform.SetParent(tile.TileObject.transform);
            _mainTower.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            _mainTower.transform.localScale = new Vector3(1, 1, 1);
        }

        public void CreateEnemySpawnPoint(int x, int y)
        {
            var poolKey = CFPoolKeys.FromIdOrName(_environmentData.EnemySpawnGateKey);
            var tile = _tileArray[x, y];
            _enemySpawnGate = _poolManager.GetGameObject(poolKey);
            _enemySpawnGate.transform.SetParent(tile.TileObject.transform);
            _enemySpawnGate.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            _enemySpawnGate.transform.localScale = new Vector3(1, 1, 1);
        }

        public async UniTask<List<Tile>> CreatePath(CancellationToken cancellationToken)
        {
            PathGenerator mapGenerator = new();
            mapGenerator.Initialize(_environmentData);

            var road = mapGenerator.GeneratePath(_startingX, _startingY);
            var path = new List<Tile>();
            var pathTileKey = CFPoolKeys.FromIdOrName(_environmentData.PathTileKey);

            foreach (var tilePosition in road)
            {
                var tile = _tileArray[tilePosition.x, tilePosition.y];
                var pathObject = _poolManager.GetGameObject(pathTileKey);
                pathObject.transform.SetParent(tile.TileObject.transform);
                pathObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
                pathObject.transform.localScale = new Vector3(1, 1, 1);
                _pathsList.Add(pathObject);
                path.Add(tile);

                await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);
            }

            path.Reverse();

            return path;
        }

        public void CreatePlayerUnitTiles(List<Tile> path)
        {
            var list = new List<Tile>();

            for (var i = 1; i < path.Count - 2; i++)
            {
                var tile = path[i];

                Tile right = null;
                Tile left = null;
                Tile up = null;
                Tile down = null;

                if (tile.Index.x + 1 < _totalWidth - 1)
                {
                    right = _tileArray[tile.Index.x + 1, tile.Index.y];
                }

                if (tile.Index.x - 1 >= 0)
                {
                    left = _tileArray[tile.Index.x - 1, tile.Index.y];
                }

                if (tile.Index.y + 1 < _totalHeight - 1)
                {
                    up = _tileArray[tile.Index.x, tile.Index.y + 1];
                }

                if (tile.Index.y - 1 >= 0)
                {
                    down = _tileArray[tile.Index.x, tile.Index.y - 1];
                }

                if (IsCellValidForPassive(path, list, right))
                {
                    list.Add(right);
                }

                if (IsCellValidForPassive(path, list, left))
                {
                    list.Add(left);
                }

                if (IsCellValidForPassive(path, list, up))
                {
                    list.Add(up);
                }

                if (IsCellValidForPassive(path, list, down))
                {
                    list.Add(down);
                }
            }

            var playerUnitKey = CFPoolKeys.FromIdOrName(_environmentData.PlayerUnitTileKey);

            foreach (var tilePosition in list)
            {
                var tile = _tileArray[tilePosition.Index.x, tilePosition.Index.y];
                var pathObject = _poolManager.GetGameObject(playerUnitKey);
                pathObject.transform.SetParent(tile.TileObject.transform);
                pathObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
                pathObject.transform.localScale = new Vector3(1, 1, 1);
                _playerUnitTilesList.Add(pathObject);

                pathObject.GetComponent<PassiveTile>().Initialize(tile);
            }
        }

        private bool IsCellValidForPassive(List<Tile> path, List<Tile> list, Tile tile)
        {
            var x = tile.Index.x;
            var y = tile.Index.y;

            return tile != null && x >= _startingX && x <= _startingX + _playableWidth && y >= _startingY && y <= _startingY + _playableHeight && !path.Contains(tile) && !list.Contains(tile);
        }

        private void DisposeGroundTiles()
        {
            for (var i = 0; i < _groundTilesList.Count; i++)
            {
                _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(_environmentData.GroundTileKey), _groundTilesList[i]);
            }

            _groundTilesList.Clear();
        }

        private void DisposeMainTower()
        {
            _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(_environmentData.MainTowerKey), _mainTower);
            _mainTower = null;
        }

        private void DisposeEnemySpawnPoint()
        {
            _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(_environmentData.EnemySpawnGateKey), _enemySpawnGate);
            _enemySpawnGate = null;
        }

        private void DisposePlayerUnitTiles()
        {
            for (var i = 0; i < _playerUnitTilesList.Count; i++)
            {
                _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(_environmentData.PlayerUnitTileKey), _playerUnitTilesList[i]);
            }

            _playerUnitTilesList.Clear();
        }

        private void DisposePath()
        {
            for (var i = 0; i < _pathsList.Count; i++)
            {
                _poolManager.SafeReleaseObject(CFPoolKeys.FromIdOrName(_environmentData.PathTileKey), _pathsList[i]);
            }

            _pathsList.Clear();
        }
    }
}
