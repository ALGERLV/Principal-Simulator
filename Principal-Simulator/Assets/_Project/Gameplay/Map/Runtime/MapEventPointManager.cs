using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Managers;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Runtime
{
    public class MapEventPointManager : MonoBehaviour
    {
        private List<MapEventPoint> activePoints = new List<MapEventPoint>();

        public IReadOnlyList<MapEventPoint> ActivePoints => activePoints;

        public void SpawnEventPoints(LevelConfig config, MapManager mapManager)
        {
            ClearAll();

            var container = new GameObject("[EventPoints]");
            container.transform.SetParent(transform);

            foreach (var data in config.EventPoints)
            {
                var coord = new MapHexCoord(data.Q, data.R);
                Vector3 worldPos = mapManager.CoordToWorldPosition(coord);

                var go = new GameObject();
                go.transform.SetParent(container.transform);

                var point = go.AddComponent<MapEventPoint>();
                point.Initialize(data, worldPos);

                activePoints.Add(point);
            }

            Debug.Log($"[MapEventPointManager] 生成了 {activePoints.Count} 个事件点");
        }

        public void ClearAll()
        {
            foreach (var point in activePoints)
            {
                if (point != null && point.gameObject != null)
                    Destroy(point.gameObject);
            }
            activePoints.Clear();

            var container = transform.Find("[EventPoints]");
            if (container != null)
                Destroy(container.gameObject);
        }
    }
}
