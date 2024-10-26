using System.Collections.Generic;
using CerberusFramework.Managers.Data.Storages;
using UnityEngine;

namespace CFGameClient.Managers.Data
{
    public class GameSessionSaveStorage : IStorage
    {
        public bool GameplayFinished = false;
        public int LevelRandomSeed;

        public int HighScore;

        public int Difficulty = 0;
        public int Gold;
        public int CurrentScore;
        public List<Vector2Int> PlacedPlayerUnitPositions = new();
        public List<int> PlacedPlayerUnitPoolKeys = new();
    }
}
