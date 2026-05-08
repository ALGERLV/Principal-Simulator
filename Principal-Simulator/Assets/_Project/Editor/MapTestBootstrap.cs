using UnityEngine;
using TBS.Map.Managers;
using TBS.Map.Runtime;

namespace TBS.Map.Editor
{
    /// <summary>
    /// 地图测试场景快速启动器 - 自动初始化地图系统
    /// 将此脚本挂载到场景中的任意 GameObject 上，运行时会自动调用地图初始化
    /// </summary>
    public class MapTestBootstrap : MonoBehaviour
    {
        [Header("测试选项")]
        [Tooltip("是否自动开始游戏（调用 MapManager.InitializeMap）")]
        public bool autoInitializeOnStart = true;

        [Tooltip("等待几帧后再初始化（确保所有组件就绪）")]
        public int delayFrames = 2;

        private void Start()
        {
            if (autoInitializeOnStart)
            {
                StartCoroutine(InitializeAfterDelay());
            }
        }

        private System.Collections.IEnumerator InitializeAfterDelay()
        {
            // 等待指定帧数，确保所有 Awake 都执行完毕
            for (int i = 0; i < delayFrames; i++)
            {
                yield return null;
            }

            // 获取或创建 MapManager
            MapManager manager = MapManager.Instance;
            if (manager == null)
            {
                Debug.Log("[MapTestBootstrap] 未找到 MapManager，正在创建...");
                GameObject go = new GameObject("MapManager");
                manager = go.AddComponent<MapManager>();
            }

            // 确保 MapTerrainGrid 存在
            MapTerrainGrid grid = FindObjectOfType<MapTerrainGrid>();
            if (grid == null)
            {
                Debug.Log("[MapTestBootstrap] 未找到 MapTerrainGrid，正在创建...");
                GameObject go = new GameObject("MapTerrainGrid");
                go.transform.SetParent(manager.transform);
                grid = go.AddComponent<MapTerrainGrid>();
            }

            // 强制初始化地图
            Debug.Log("[MapTestBootstrap] 开始初始化地图...");
            manager.InitializeMap();

            Debug.Log($"[MapTestBootstrap] 地图初始化完成！共 {grid.TileCount} 个地块");
        }
    }
}
