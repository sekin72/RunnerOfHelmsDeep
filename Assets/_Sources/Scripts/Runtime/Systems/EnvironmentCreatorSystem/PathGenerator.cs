using System.Collections.Generic;
using GameClient.GameData;
using UnityEngine;

namespace GameClient.Runtime.Systems.EnvironmentCreatorSystem
{
    public class PathGenerator
    {
        private int _width;
        private int _height;
        private List<Vector2Int> _path;

        public void Initialize(EnvironmentData environmentData)
        {
            _width = environmentData.PlayableWidth;
            _height = environmentData.PlayableHeight;
        }

        public List<Vector2Int> GeneratePath(int startX, int startY)
        {
            _path = new List<Vector2Int>();

            var x = Random.Range(0, _width);
            var y = 0;
            var straightCount = 0;

            _path.Add(new Vector2Int(startX + x, startY + y++));
            _path.Add(new Vector2Int(startX + x, startY + y));

            while (y < _height)
            {
                var validMove = false;

                while (!validMove)
                {
                    var rnd = Random.Range(0, 3);

                    if ((rnd == 0 || y % 2 == 0) && straightCount < 4)
                    {
                        y++;
                        straightCount++;
                        validMove = true;
                    }
                    else if (rnd == 1 && IsCellFree(startX + x + 1, startY + y) && x < _width)
                    {
                        x++;
                        straightCount = 0;
                        validMove = true;
                    }
                    else if (rnd == 2 && IsCellFree(startX + x - 1, startY + y) && x > 0)
                    {
                        x--;
                        straightCount = 0;
                        validMove = true;
                    }
                }

                _path.Add(new Vector2Int(startX + x, startY + y));
            }

            return _path;
        }

        private bool IsCellFree(int x, int y)
        {
            return !_path.Contains((Vector2Int.right * x) + (Vector2Int.up * y));
        }
    }
}