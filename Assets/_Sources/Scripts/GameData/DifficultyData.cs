using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient.GameData
{
    [CreateAssetMenu(fileName = "DifficultyData", menuName = "GameClient/DifficultyData", order = 2)]
    public class DifficultyData : ScriptableObject
    {
        public List<WaveData> WaveDatas = new()
    {
        new WaveData(10, 5f, new List<int> { 200 }),
        new WaveData(25, 4.5f, new List<int> { 200 }),
        new WaveData(50, 4f, new List<int> { 200, 201 }),
        new WaveData(100, 3.5f, new List<int> { 200, 201 }),
        new WaveData(200, 2.5f, new List<int> { 200, 201 }),
        new WaveData(300, 2f, new List<int> { 200, 201, 202 }),
        new WaveData(500, 1.5f, new List<int> { 200, 201, 202 }),
        new WaveData(750, 1f, new List<int> { 200, 201, 202 }),
    };
    }

    [Serializable]
    public struct WaveData
    {
        public int Score;
        public float TimeInterval;
        public List<int> EnemyPoolKeys;

        public WaveData(int score, float timeInterval, List<int> enemyPoolKeys)
        {
            Score = score;
            TimeInterval = timeInterval;
            EnemyPoolKeys = enemyPoolKeys;
        }
    }
}
