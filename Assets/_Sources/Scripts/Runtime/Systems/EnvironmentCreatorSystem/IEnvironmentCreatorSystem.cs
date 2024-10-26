using System.Collections.Generic;
using CerberusFramework.Core.Systems;
using UnityEngine;

namespace GameClient.Runtime.Systems.EnvironmentCreatorSystem
{
    public interface IEnvironmentCreatorSystem : IGameSystem
    {
        public Tile GetTile(Vector2Int index);
        public GameObject GetMainTower();
        public List<Tile> Path { get; }
    }
}
