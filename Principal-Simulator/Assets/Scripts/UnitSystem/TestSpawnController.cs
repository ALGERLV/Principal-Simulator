using UnityEngine;
using TBS.Map.Tools;
using TBS.Map.Components;

namespace TBS.UnitSystem
{
    /// <summary>
    /// 测试专用：按 K 键在地图中央生成一枚测试兵牌
    /// 挂载到场景中任意 GameObject 即可（建议挂到 GameManager）
    /// </summary>
    public class TestSpawnController : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("按下此键生成测试单位")]
        public KeyCode spawnKey = KeyCode.K;

        [Tooltip("生成坐标（留 (0,0) 时自动选地图中央附近的第一个可用格）")]
        public HexCoord spawnCoord = new HexCoord(5, 5);

        [Tooltip("每格代表的公里数（用于显示移动时间参考）")]
        [SerializeField] private bool autoFindCenter = true;

        private UnitTokenSpawner spawner;
        private HexGrid hexGrid;
        private bool spawned;

        void Start()
        {
            hexGrid = FindObjectOfType<HexGrid>();
            spawner = FindObjectOfType<UnitTokenSpawner>();

            // 若场景中没有 Spawner，动态添加一个
            if (spawner == null)
            {
                spawner = gameObject.AddComponent<UnitTokenSpawner>();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(spawnKey))
            {
                SpawnTestUnit();
            }
        }

        void SpawnTestUnit()
        {
            if (hexGrid == null) hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null)
            {
                Debug.LogError("TestSpawnController: 场景中没有 HexGrid");
                return;
            }

            HexCoord coord = spawnCoord;

            if (autoFindCenter || !hexGrid.HasTile(coord))
            {
                coord = FindNearCenter(hexGrid);
            }

            var token = spawner.SpawnUnit(coord);
            if (token != null)
            {
                Debug.Log($"[测试] 生成单位 \"{token.UnitName}\" @ {coord}  " +
                          $"行军速度={token.UnitLogic?.MoveSpeedKmPerDay ?? 0}km/day");
                spawned = true;
            }
        }

        static HexCoord FindNearCenter(HexGrid grid)
        {
            int cx = grid.Width  / 2;
            int cy = grid.Height / 2;

            // 从中心向外螺旋搜索第一个空闲格
            for (int r = 0; r <= Mathf.Max(grid.Width, grid.Height); r++)
            {
                var coord = new HexCoord(cx, cy);
                var spiral = coord.GetSpiral(r);
                foreach (var c in spiral)
                {
                    var tile = grid.GetTile(c);
                    if (tile != null && !tile.IsOccupied)
                        return c;
                }
            }
            return new HexCoord(cx, cy);
        }
    }
}
