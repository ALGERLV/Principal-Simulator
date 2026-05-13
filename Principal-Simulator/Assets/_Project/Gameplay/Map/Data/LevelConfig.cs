using System;
using System.Collections.Generic;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Data
{
    public enum MapEventPointType
    {
        KMTReinforcement,
        JapanReinforcement,
        VictoryPoint
    }

    [Serializable]
    public struct TileTerrainEntry
    {
        public int Q;
        public int R;
        public string TerrainId;
    }

    [Serializable]
    public struct MapEventPointData
    {
        public int Q;
        public int R;
        public MapEventPointType PointType;
        public string PointName;
        public int ScoreValue;
    }

    [CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Config", order = 2)]
    public class LevelConfig : ScriptableObject
    {
        [Header("基本信息")]
        public string LevelId;
        public string LevelName;
        [TextArea(2, 4)] public string Description;

        [Header("地图参数")]
        public int MapWidth = 20;
        public int MapHeight = 15;
        public float HexSize = 1f;
        public HexOrientation Orientation = HexOrientation.PointyTop;
        public GridShape GridShape = GridShape.Rectangle;

        [Header("地形配置")]
        public TerrainData DefaultTerrain;
        public List<TileTerrainEntry> TileTerrains = new List<TileTerrainEntry>();

        [Header("河流配置")]
        public List<MapLinkType> Rivers = new List<MapLinkType>();

        [Header("事件点")]
        public List<MapEventPointData> EventPoints = new List<MapEventPointData>();
    }
}
