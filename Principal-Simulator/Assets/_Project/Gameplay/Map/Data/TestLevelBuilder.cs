using System.Collections.Generic;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Data
{
    public static class TestLevelBuilder
    {
        public static LevelConfig Build()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.LevelId = "test_level";
            config.LevelName = "测试关卡";
            config.Description = "淞沪会战前期 — 闸北·虹口战区";
            config.MapWidth = 20;
            config.MapHeight = 15;
            config.HexSize = 1f;
            config.Orientation = HexOrientation.PointyTop;
            config.GridShape = GridShape.Rectangle;

            config.TileTerrains = BuildTerrainLayout();
            config.Rivers = BuildRivers();
            config.EventPoints = BuildEventPoints();

            return config;
        }

        static List<TileTerrainEntry> BuildTerrainLayout()
        {
            var list = new List<TileTerrainEntry>();

            // 默认全部是平原 (plain), 只需标注非平原的格子

            // ─── 北部城区 (行0-3): 闸北 ───
            for (int q = 0; q < 8; q++)
                for (int r = 0; r <= 3; r++)
                    list.Add(new TileTerrainEntry { Q = q, R = r, TerrainId = "town" });

            // 闸北区散布的树林/公园
            list.Add(new TileTerrainEntry { Q = 2, R = 1, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 6, R = 2, TerrainId = "forest" });

            // ─── 中北部 (行2-6): 虹口·日本区 ───
            for (int q = 8; q < 16; q++)
                for (int r = 1; r <= 5; r++)
                    list.Add(new TileTerrainEntry { Q = q, R = r, TerrainId = "town" });

            // 虹口公园 (森林)
            list.Add(new TileTerrainEntry { Q = 8, R = 3, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 9, R = 3, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 8, R = 4, TerrainId = "forest" });

            // ─── 东部: 杨树浦方向 ───
            for (int q = 16; q < 20; q++)
                for (int r = 0; r <= 6; r++)
                    list.Add(new TileTerrainEntry { Q = q, R = r, TerrainId = "town" });

            // ─── 苏州河 (行7-8 部分水域) ───
            for (int q = 0; q <= 15; q++)
                list.Add(new TileTerrainEntry { Q = q, R = 7, TerrainId = "water" });
            // 苏州河弯曲段
            list.Add(new TileTerrainEntry { Q = 0, R = 8, TerrainId = "water" });
            list.Add(new TileTerrainEntry { Q = 1, R = 8, TerrainId = "water" });

            // ─── 南部 (行8-14): 公共租界/法租界 ───
            for (int q = 2; q < 16; q++)
                for (int r = 8; r <= 14; r++)
                    list.Add(new TileTerrainEntry { Q = q, R = r, TerrainId = "town" });

            // 南部散布平原 (不添加, 用默认)
            // 四行仓库附近
            list.Add(new TileTerrainEntry { Q = 3, R = 9, TerrainId = "town" });
            list.Add(new TileTerrainEntry { Q = 3, R = 10, TerrainId = "town" });

            // ─── 黄浦江 (右下角) ───
            for (int q = 16; q < 20; q++)
                for (int r = 7; r <= 14; r++)
                    list.Add(new TileTerrainEntry { Q = q, R = r, TerrainId = "water" });

            // 黄浦江西岸码头 (town)
            list.Add(new TileTerrainEntry { Q = 15, R = 8, TerrainId = "town" });
            list.Add(new TileTerrainEntry { Q = 15, R = 9, TerrainId = "town" });

            // ─── 北部开阔地 (部分平原已经是默认, 补充森林) ───
            list.Add(new TileTerrainEntry { Q = 0, R = 4, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 1, R = 5, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 0, R = 5, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 0, R = 6, TerrainId = "forest" });
            list.Add(new TileTerrainEntry { Q = 1, R = 6, TerrainId = "forest" });

            return list;
        }

        static List<MapLinkType> BuildRivers()
        {
            var list = new List<MapLinkType>();

            // 苏州河: 从西到东沿行7
            for (int q = 0; q < 15; q++)
            {
                list.Add(new MapLinkType
                {
                    From = new MapHexCoord(q, 7),
                    To = new MapHexCoord(q + 1, 7),
                    Type = MapRouteType.River,
                    WidthWorld = 0.15f,
                    Passable = false
                });
            }

            // 苏州河弯曲段连接
            list.Add(new MapLinkType
            {
                From = new MapHexCoord(0, 7),
                To = new MapHexCoord(0, 8),
                Type = MapRouteType.River,
                WidthWorld = 0.15f,
                Passable = false
            });
            list.Add(new MapLinkType
            {
                From = new MapHexCoord(0, 8),
                To = new MapHexCoord(1, 8),
                Type = MapRouteType.River,
                WidthWorld = 0.15f,
                Passable = false
            });

            // 黄浦江: 从北到南沿 q=16
            for (int r = 7; r < 14; r++)
            {
                list.Add(new MapLinkType
                {
                    From = new MapHexCoord(16, r),
                    To = new MapHexCoord(16, r + 1),
                    Type = MapRouteType.River,
                    WidthWorld = 0.25f,
                    Passable = false
                });
            }

            // 苏州河汇入黄浦江
            list.Add(new MapLinkType
            {
                From = new MapHexCoord(15, 7),
                To = new MapHexCoord(16, 7),
                Type = MapRouteType.River,
                WidthWorld = 0.2f,
                Passable = false
            });

            return list;
        }

        static List<MapEventPointData> BuildEventPoints()
        {
            return new List<MapEventPointData>
            {
                // ─── 国军增援点 (地图边缘) ───
                new MapEventPointData
                {
                    Q = 0, R = 0,
                    PointType = MapEventPointType.KMTReinforcement,
                    PointName = "孙元良部(88师)",
                    ScoreValue = 0
                },
                new MapEventPointData
                {
                    Q = 5, R = 0,
                    PointType = MapEventPointType.KMTReinforcement,
                    PointName = "88师264旅",
                    ScoreValue = 0
                },
                new MapEventPointData
                {
                    Q = 19, R = 0,
                    PointType = MapEventPointType.KMTReinforcement,
                    PointName = "王敬久部(87师)",
                    ScoreValue = 0
                },

                // ─── 日军增援点 (地图边缘) ───
                new MapEventPointData
                {
                    Q = 19, R = 7,
                    PointType = MapEventPointType.JapanReinforcement,
                    PointName = "海军陆战队",
                    ScoreValue = 0
                },
                new MapEventPointData
                {
                    Q = 19, R = 14,
                    PointType = MapEventPointType.JapanReinforcement,
                    PointName = "海军增援",
                    ScoreValue = 0
                },

                // ─── 胜利点 ───
                new MapEventPointData
                {
                    Q = 8, R = 3,
                    PointType = MapEventPointType.VictoryPoint,
                    PointName = "虹口公园",
                    ScoreValue = 30
                },
                new MapEventPointData
                {
                    Q = 3, R = 10,
                    PointType = MapEventPointType.VictoryPoint,
                    PointName = "四行仓库",
                    ScoreValue = 20
                },
                new MapEventPointData
                {
                    Q = 5, R = 7,
                    PointType = MapEventPointType.VictoryPoint,
                    PointName = "上海北站",
                    ScoreValue = 25
                },
                new MapEventPointData
                {
                    Q = 12, R = 4,
                    PointType = MapEventPointType.VictoryPoint,
                    PointName = "日本海军司令部",
                    ScoreValue = 35
                }
            };
        }
    }
}
