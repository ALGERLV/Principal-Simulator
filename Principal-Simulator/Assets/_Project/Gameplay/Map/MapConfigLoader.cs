using System;
using System.Collections.Generic;
using System.IO;
using TBS.Map.Components;
using TBS.Map.Data;
using TBS.Map.Tools;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Tools
{
    /// <summary>
    /// 地图配置加载器 - 从JSON文件加载地图配置
    /// </summary>
    public static class MapConfigLoader
    {
        #region Load Methods

        /// <summary>
        /// 从JSON文件路径加载地图配置
        /// </summary>
        public static MapConfig LoadFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"MapConfigLoader: 文件不存在 {jsonPath}");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            return ParseJson(json);
        }

        /// <summary>
        /// 从Resources加载地图配置
        /// </summary>
        public static MapConfig LoadFromResources(string resourcePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"MapConfigLoader: 无法加载资源 {resourcePath}");
                return null;
            }

            return ParseJson(textAsset.text);
        }

        /// <summary>
        /// 从StreamingAssets加载地图配置
        /// </summary>
        public static MapConfig LoadFromStreamingAssets(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Maps", fileName);
            return LoadFromJson(path);
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// 将地图配置保存为JSON
        /// </summary>
        public static void SaveToJson(MapConfig config, string outputPath)
        {
            var serializable = new SerializableMapConfig(config);
            string json = JsonUtility.ToJson(serializable, true);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, json);

            Debug.Log($"MapConfigLoader: 配置已保存到 {outputPath}");
        }

        /// <summary>
        /// 将HexGrid当前状态保存为配置文件
        /// </summary>
        public static void SaveGridToJson(HexGrid grid, string outputPath)
        {
            var config = CreateConfigFromGrid(grid);
            SaveToJson(config, outputPath);
        }

        #endregion

        #region Helper Methods

        private static MapConfig ParseJson(string json)
        {
            try
            {
                var serializable = JsonUtility.FromJson<SerializableMapConfig>(json);
                return serializable.ToMapConfig();
            }
            catch (Exception e)
            {
                Debug.LogError($"MapConfigLoader: JSON解析错误 - {e.Message}");
                return null;
            }
        }

        private static MapConfig CreateConfigFromGrid(HexGrid grid)
        {
            var config = new MapConfig
            {
                Width = grid.Width,
                Height = grid.Height,
                Shape = GridShape.Custom,
                DefaultTerrain = null,
                TerrainOverrides = new Dictionary<HexCoord, TerrainData>()
            };

            foreach (var tile in grid.AllTiles)
            {
                config.TerrainOverrides[tile.Coord] = tile.TerrainData;
            }

            return config;
        }

        #endregion
    }

    #region Serializable Types

    /// <summary>
    /// 可序列化的地图配置（用于JSON转换）
    /// </summary>
    [Serializable]
    public class SerializableMapConfig
    {
        public int width;
        public int height;
        public string shape;
        public string defaultTerrainId;
        public List<SerializableTerrainOverride> terrainOverrides;

        public SerializableMapConfig() { }

        public SerializableMapConfig(MapConfig config)
        {
            width = config.Width;
            height = config.Height;
            shape = config.Shape.ToString();
            defaultTerrainId = config.DefaultTerrain?.TerrainId;

            terrainOverrides = new List<SerializableTerrainOverride>();
            if (config.TerrainOverrides != null)
            {
                foreach (var kvp in config.TerrainOverrides)
                {
                    terrainOverrides.Add(new SerializableTerrainOverride
                    {
                        q = kvp.Key.Q,
                        r = kvp.Key.R,
                        terrainId = kvp.Value?.TerrainId ?? "unknown"
                    });
                }
            }
        }

        public MapConfig ToMapConfig()
        {
            var config = new MapConfig
            {
                Width = width,
                Height = height,
                Shape = ParseShape(shape),
                TerrainOverrides = new Dictionary<HexCoord, TerrainData>()
            };

            // 加载默认地形
            if (!string.IsNullOrEmpty(defaultTerrainId))
            {
                config.DefaultTerrain = LoadTerrainAsset(defaultTerrainId);
            }

            // 加载地形覆盖
            if (terrainOverrides != null)
            {
                foreach (var ov in terrainOverrides)
                {
                    var coord = new HexCoord(ov.q, ov.r);
                    var terrain = LoadTerrainAsset(ov.terrainId) ?? config.DefaultTerrain;
                    config.TerrainOverrides[coord] = terrain;
                }
            }

            return config;
        }

        private static GridShape ParseShape(string shapeStr)
        {
            if (Enum.TryParse<GridShape>(shapeStr, out var shape))
            {
                return shape;
            }
            return GridShape.Rectangle;
        }

        private static TerrainData LoadTerrainAsset(string terrainId)
        {
            // 从Resources加载地形资源
            var terrains = Resources.LoadAll<TerrainData>("Terrain");
            foreach (var terrain in terrains)
            {
                if (terrain.TerrainId == terrainId)
                {
                    return terrain;
                }
            }

            // 尝试通过ID直接加载
            return Resources.Load<TerrainData>($"Terrain/{terrainId}");
        }
    }

    [Serializable]
    public class SerializableTerrainOverride
    {
        public int q;
        public int r;
        public string terrainId;
    }

    #endregion
}
