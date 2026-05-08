using UnityEngine;
using TBS.Map.Tools;
using TBS.Map.Components;

namespace TBS.UnitSystem
{
    public class UnitTokenSpawner : MonoBehaviour
    {
        [Header("生成设置")]
        public float tokenHeight = 0.05f;

        [Header("默认属性（无 UnitData 时使用）")]
        public string DefaultUnitId   = "88S_262R";
        public string DefaultUnitName = "88师 262团";

        // 用 int 存枚举值，避免跨命名空间引用问题
        // Faction: 0=KMT 1=Japan 2=PLA
        public int DefaultFaction = 0;
        // UnitTier: 0=HQ 1=Regiment
        public int DefaultTier = 1;
        // UnitGrade: 0=KMT_Elite 1=KMT_Regular 2=KMT_Militia 3=Japan 4=PLA_Elite 5=PLA_Regular
        public int DefaultGrade = 0;

        [Header("战斗属性")]
        public int   AttackPower       = 7;
        public int   DefensePower      = 6;
        public float MoveSpeedKmPerDay = 25f;
        public int   Firepower         = 4;

        [Header("资源属性")]
        public int Strength = 5;
        public int Morale   = 80;
        public int Supply   = 4;

        // ─────────────────────────────────────────────────────────

        public UnitToken SpawnUnit(HexCoord spawnCoord, TBS.Unit.UnitData unitData = null)
        {
            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null) { Debug.LogError("UnitTokenSpawner: 未找到 HexGrid"); return null; }

            var tile = hexGrid.GetTile(spawnCoord);
            if (tile == null)
            {
                tile = FindFirstAvailableTile(hexGrid);
                if (tile == null) { Debug.LogError("UnitTokenSpawner: 无可用地块"); return null; }
                spawnCoord = tile.Coord;
            }
            if (tile.IsOccupied) { Debug.LogWarning($"UnitTokenSpawner: {spawnCoord} 已被占据"); return null; }

            Vector3 worldPos = hexGrid.CoordToWorldPosition(spawnCoord);
            worldPos.y = tokenHeight;

            var unitsContainer = GameObject.Find("[Units]");
            if (unitsContainer == null)
                unitsContainer = new GameObject("[Units]");

            var root = new GameObject($"Unit_{DefaultUnitName}");
            root.transform.SetParent(unitsContainer.transform, false);
            root.transform.position = worldPos;
            root.tag = "UnitToken";

            var unit  = root.AddComponent<TBS.Unit.Unit>();
            var token = root.AddComponent<UnitToken>();

            if (unitData != null)
                unit.Initialize(unitData, spawnCoord);
            else
                unit.InitializeRuntime(BuildRuntimeParams(), spawnCoord);

            BuildVisuals(root, token);
            token.InitializeOnTile(spawnCoord);

            Debug.Log($"UnitTokenSpawner: 生成 {DefaultUnitName} @ {spawnCoord}  速度={unit.MoveSpeedKmPerDay}km/day");
            return token;
        }

        /// <summary>
        /// 使用外部传入的 UnitRuntimeParams 生成单位
        /// </summary>
        public UnitToken SpawnUnit(HexCoord spawnCoord, TBS.Unit.UnitRuntimeParams runtimeParams)
        {
            if (runtimeParams == null)
            {
                Debug.LogError("UnitTokenSpawner: runtimeParams 为空");
                return null;
            }

            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null) { Debug.LogError("UnitTokenSpawner: 未找到 HexGrid"); return null; }

            var tile = hexGrid.GetTile(spawnCoord);
            if (tile == null)
            {
                tile = FindFirstAvailableTile(hexGrid);
                if (tile == null) { Debug.LogError("UnitTokenSpawner: 无可用地块"); return null; }
                spawnCoord = tile.Coord;
            }
            if (tile.IsOccupied) { Debug.LogWarning($"UnitTokenSpawner: {spawnCoord} 已被占据"); return null; }

            Vector3 worldPos = hexGrid.CoordToWorldPosition(spawnCoord);
            worldPos.y = tokenHeight;

            var unitsContainer = GameObject.Find("[Units]");
            if (unitsContainer == null)
                unitsContainer = new GameObject("[Units]");

            var root = new GameObject($"Unit_{runtimeParams.DisplayName}");
            root.transform.SetParent(unitsContainer.transform, false);
            root.transform.position = worldPos;
            root.tag = "UnitToken";

            var unit  = root.AddComponent<TBS.Unit.Unit>();
            var token = root.AddComponent<UnitToken>();

            // 使用传入的 RuntimeParams
            unit.InitializeRuntime(runtimeParams, spawnCoord);

            BuildVisuals(root, token);
            token.InitializeOnTile(spawnCoord);

            Debug.Log($"UnitTokenSpawner: 生成 {runtimeParams.DisplayName} @ {spawnCoord}  速度={unit.MoveSpeedKmPerDay}km/day");
            return token;
        }

        // ─────────────────────────────────────────────────────────

        TBS.Unit.UnitRuntimeParams BuildRuntimeParams() => new TBS.Unit.UnitRuntimeParams
        {
            UnitId            = DefaultUnitId,
            DisplayName       = DefaultUnitName,
            Faction           = (TBS.Unit.Faction)DefaultFaction,
            Tier              = (TBS.Unit.UnitTier)DefaultTier,
            Grade             = (TBS.Unit.UnitGrade)DefaultGrade,
            AttackPower       = AttackPower,
            DefensePower      = DefensePower,
            MoveSpeedKmPerDay = MoveSpeedKmPerDay,
            Firepower         = Firepower,
            InitialStrength   = Strength,
            InitialMorale     = Morale,
            InitialSupply     = Supply,
            ShakenMoraleThreshold     = 40,
            RoutedMoraleThreshold     = 20,
            RoutedStrengthThreshold   = 1,
            MoraleRecoveryPerHour     = 2.5f,
            RecuperationSpeedModifier = 1.0f
        };

        void BuildVisuals(GameObject root, UnitToken token)
        {
            var col    = root.AddComponent<BoxCollider>();
            col.size   = new Vector3(1.6f, 0.2f, 1.6f);
            col.center = new Vector3(0f, 0.1f, 0f);

            token.BaseRenderer   = MakeCylinder("TokenBase",   root.transform, Vector3.zero,             new Vector3(0.8f,  0.05f, 0.8f)).GetComponent<MeshRenderer>();
            token.BorderRenderer = MakeCylinder("TokenBorder", root.transform, new Vector3(0, 0.06f, 0), new Vector3(0.85f, 0.02f, 0.85f)).GetComponent<MeshRenderer>();

            var sym = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sym.name = "TokenSymbol_BG";
            sym.transform.SetParent(root.transform);
            sym.transform.localPosition = new Vector3(0, 0.08f, 0);
            sym.transform.localScale    = new Vector3(0.25f, 0.05f, 0.25f);
            DestroyImmediate(sym.GetComponent<SphereCollider>());
            token.SymbolRenderer = sym.GetComponent<MeshRenderer>();
        }

        static GameObject MakeCylinder(string name, Transform parent, Vector3 lPos, Vector3 lScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = lPos;
            go.transform.localScale    = lScale;
            DestroyImmediate(go.GetComponent<CapsuleCollider>());
            return go;
        }


        HexTile FindFirstAvailableTile(HexGrid hexGrid)
        {
            foreach (var tile in hexGrid.AllTiles)
                if (!tile.IsOccupied) return tile;
            return null;
        }
    }
}
