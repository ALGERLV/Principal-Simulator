using System.Collections.Generic;
using TBS.Map.Managers;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Runtime
{
    public static class HexPathfinding
    {
        public static List<MapHexCoord> FindPath(MapHexCoord start, MapHexCoord goal, MapManager mapManager)
        {
            if (mapManager == null) return null;

            var startTile = mapManager.GetTile(start);
            var goalTile = mapManager.GetTile(goal);
            if (startTile == null || goalTile == null) return null;
            if (!goalTile.IsPassable) return null;

            var openList = new List<(float f, MapHexCoord coord)>();
            var cameFrom = new Dictionary<MapHexCoord, MapHexCoord>();
            var gScore = new Dictionary<MapHexCoord, float>();
            var closed = new HashSet<MapHexCoord>();

            gScore[start] = 0;
            float h = start.DistanceTo(goal);
            openList.Add((h, start));

            while (openList.Count > 0)
            {
                int minIdx = 0;
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].f < openList[minIdx].f)
                        minIdx = i;
                }

                var current = openList[minIdx].coord;
                openList.RemoveAt(minIdx);

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                closed.Add(current);

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (closed.Contains(neighbor)) continue;

                    var tile = mapManager.GetTile(neighbor);
                    if (tile == null || !tile.IsPassable) continue;
                    if (tile.IsOccupied && neighbor != goal) continue;

                    float tentativeG = gScore[current] + tile.MovementCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + neighbor.DistanceTo(goal);
                        openList.Add((f, neighbor));
                    }
                }
            }

            return null;
        }

        static List<MapHexCoord> ReconstructPath(Dictionary<MapHexCoord, MapHexCoord> cameFrom, MapHexCoord current)
        {
            var path = new List<MapHexCoord> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}
