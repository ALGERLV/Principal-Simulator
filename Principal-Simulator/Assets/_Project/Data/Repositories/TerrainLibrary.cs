using System.Collections.Generic;
using System.Linq;
using TBS.Map.Data;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Repositories
{
    /// <summary>
    /// 地形库 - 自动加载并管理所有地形资源
    /// </summary>
    public static class TerrainLibrary
    {
        private static Dictionary<string, TerrainData> terrainMap = new Dictionary<string, TerrainData>();
        private static TerrainData[] allTerrains;
        private static bool isInitialized = false;

        /// <summary>
        /// 初始化地形库
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            terrainMap.Clear();

            // 自动加载所有地形资源
            allTerrains = Resources.LoadAll<TerrainData>("ScriptableObjects/Terrain");

            foreach (var terrain in allTerrains)
            {
                if (terrain != null)
                {
                    terrainMap[terrain.TerrainId] = terrain;
                    Debug.Log($"[TerrainLibrary] 加载地形: {terrain.TerrainName} ({terrain.TerrainId})");
                }
            }

            Debug.Log($"[TerrainLibrary] 初始化完成，加载了 {allTerrains.Length} 个地形");
            isInitialized = true;
        }

        /// <summary>
        /// 通过ID获取地形
        /// </summary>
        public static TerrainData GetTerrainById(string terrainId)
        {
            if (!isInitialized)
                Initialize();

            if (terrainMap.TryGetValue(terrainId, out var terrain))
                return terrain;

            Debug.LogWarning($"[TerrainLibrary] 未找到地形ID: {terrainId}");
            return null;
        }

        /// <summary>
        /// 获取所有地形
        /// </summary>
        public static TerrainData[] GetAllTerrains()
        {
            if (!isInitialized)
                Initialize();

            return allTerrains;
        }

        /// <summary>
        /// 获取所有地形（排除特定地形）
        /// </summary>
        public static TerrainData[] GetAllTerrains(string excludeTerrainId)
        {
            if (!isInitialized)
                Initialize();

            return allTerrains.Where(t => t.TerrainId != excludeTerrainId).ToArray();
        }

        /// <summary>
        /// 获取地形总数
        /// </summary>
        public static int GetTerrainCount()
        {
            if (!isInitialized)
                Initialize();

            return allTerrains.Length;
        }

        /// <summary>
        /// 检查地形是否存在
        /// </summary>
        public static bool HasTerrain(string terrainId)
        {
            if (!isInitialized)
                Initialize();

            return terrainMap.ContainsKey(terrainId);
        }

        /// <summary>
        /// 获取随机地形（排除水域）
        /// </summary>
        public static TerrainData GetRandomTerrain(string excludeTerrainId = "water")
        {
            if (!isInitialized)
                Initialize();

            var availableTerrains = allTerrains.Where(t => t.TerrainId != excludeTerrainId && t != null).ToArray();

            if (availableTerrains.Length == 0)
                return allTerrains[0];

            int randomIndex = Random.Range(0, availableTerrains.Length);
            return availableTerrains[randomIndex];
        }
    }
}
