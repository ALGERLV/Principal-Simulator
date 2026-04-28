using UnityEngine;
using UnityEngine.UI;
using TBS.Map.Tools;
using TBS.Map.Components;

namespace TBS.UnitSystem
{
    /// <summary>
    /// 兵牌生成器 - 在地图上初始化放置兵牌
    /// </summary>
    public class UnitTokenSpawner : MonoBehaviour
    {
        [Header("生成设置")]
        public UnitToken unitPrefab; // 兵牌预制体（可选）
        public HexCoord spawnCoord = new HexCoord(5, 5); // 默认生成坐标
        public float tokenHeight = 1f; // 兵牌离地高度
        public bool autoGenerateIfNoPrefab = true; // 无预制体时自动生成

        [Header("默认单位数据")]
        public string UnitId = "88S_262R";
        public string UnitName = "88师 262团";
        public Faction faction = Faction.Nationalist;
        public UnitTier tier = UnitTier.Regiment;
        public UnitGrade grade = UnitGrade.Elite;

        [Header("战斗属性")]
        public int AttackPower = 7;
        public int DefensePower = 6;
        public float MoveSpeedKmPerDay = 25f;
        public int Firepower = 4;

        [Header("资源属性")]
        public int Strength = 5;
        public int Morale = 80;
        public int Supply = 4;

        void Start()
        {
            // 等待地图生成完成
            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null)
            {
                Debug.LogError("UnitTokenSpawner: 未找到HexGrid");
                return;
            }

            if (hexGrid.IsInitialized)
            {
                // 地图已生成，直接生成兵牌
                SpawnUnit();
            }
            else
            {
                // 订阅地图生成事件
                hexGrid.OnGridGenerated += OnGridGenerated;
                Debug.Log("UnitTokenSpawner: 等待地图生成...");
            }
        }

        void OnDestroy()
        {
            // 取消订阅
            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid != null)
            {
                hexGrid.OnGridGenerated -= OnGridGenerated;
            }
        }

        void OnGridGenerated()
        {
            Debug.Log("UnitTokenSpawner: 地图已生成，开始生成兵牌");
            SpawnUnit();
            
            // 取消订阅，避免重复生成
            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid != null)
            {
                hexGrid.OnGridGenerated -= OnGridGenerated;
            }
        }

        /// <summary>
        /// 在指定坐标生成兵牌
        /// </summary>
        public UnitToken SpawnUnit()
        {
            var hexGrid = FindObjectOfType<HexGrid>();
            if (hexGrid == null)
            {
                Debug.LogError("UnitTokenSpawner: 未找到HexGrid");
                return null;
            }

            // 检查坐标是否有效
            var tile = hexGrid.GetTile(spawnCoord);
            if (tile == null)
            {
                Debug.LogError($"UnitTokenSpawner: 坐标 {spawnCoord} 不存在地块，尝试查找替代坐标");
                // 尝试找到第一个可用地块
                tile = FindFirstAvailableTile(hexGrid);
                if (tile == null)
                {
                    Debug.LogError("UnitTokenSpawner: 无法找到可用地块");
                    return null;
                }
                spawnCoord = tile.Coord;
                Debug.Log($"UnitTokenSpawner: 使用替代坐标 {spawnCoord}");
            }

            // 检查是否已被占据
            if (tile.IsOccupied)
            {
                Debug.LogWarning($"UnitTokenSpawner: 坐标 {spawnCoord} 已被占据");
                return null;
            }

            // 计算世界位置
            Vector3 worldPos = hexGrid.CoordToWorldPosition(spawnCoord);
            worldPos.y = tokenHeight;

            // 生成兵牌
            UnitToken unit;
            if (unitPrefab != null)
            {
                unit = Instantiate(unitPrefab, worldPos, Quaternion.identity);
            }
            else if (autoGenerateIfNoPrefab)
            {
                // 自动生成完整兵牌结构
                unit = CreateCompleteUnitToken(worldPos);
            }
            else
            {
                Debug.LogError("UnitTokenSpawner: 未设置兵牌预制体且autoGenerateIfNoPrefab为false");
                return null;
            }

            // 设置兵牌名称
            unit.gameObject.name = $"UnitToken_{UnitName}";
            unit.gameObject.tag = "UnitToken";
            unit.gameObject.layer = LayerMask.NameToLayer("Unit");

            // 初始化属性
            InitializeUnitProperties(unit);

            // 初始化到地块
            unit.InitializeOnTile(spawnCoord);

            Debug.Log($"UnitTokenSpawner: 兵牌 {UnitName} 已生成于 {spawnCoord}");

            return unit;
        }

        /// <summary>
        /// 创建完整的兵牌结构
        /// </summary>
        UnitToken CreateCompleteUnitToken(Vector3 position)
        {
            // 创建根物体
            var root = new GameObject($"UnitToken_{UnitName}");
            root.transform.position = position;
            
            var unitToken = root.AddComponent<UnitToken>();

            // 在根物体添加大碰撞器用于点击检测（覆盖整个兵牌区域）
            var rootCollider = root.AddComponent<BoxCollider>();
            rootCollider.size = new Vector3(1.6f, 0.2f, 1.6f);
            rootCollider.center = new Vector3(0, 0.1f, 0);

            // 1. 创建底座 (TokenBase)
            var tokenBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tokenBase.name = "TokenBase";
            tokenBase.transform.SetParent(root.transform);
            tokenBase.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            tokenBase.transform.localPosition = Vector3.zero;
            
            // 替换圆柱碰撞器为BoxCollider
            DestroyImmediate(tokenBase.GetComponent<CapsuleCollider>());
            var baseCollider = tokenBase.AddComponent<BoxCollider>();
            baseCollider.size = new Vector3(1.6f, 0.1f, 1.6f);
            
            var baseRenderer = tokenBase.GetComponent<MeshRenderer>();
            unitToken.BaseRenderer = baseRenderer;

            // 2. 创建边框 (TokenBorder)
            var tokenBorder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tokenBorder.name = "TokenBorder";
            tokenBorder.transform.SetParent(root.transform);
            tokenBorder.transform.localScale = new Vector3(0.85f, 0.02f, 0.85f);
            tokenBorder.transform.localPosition = new Vector3(0, 0.06f, 0);
            DestroyImmediate(tokenBorder.GetComponent<CapsuleCollider>());
            
            var borderRenderer = tokenBorder.GetComponent<MeshRenderer>();
            unitToken.BorderRenderer = borderRenderer;

            // 3. 创建标识背景 (TokenSymbol_BG)
            var tokenSymbol = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tokenSymbol.name = "TokenSymbol_BG";
            tokenSymbol.transform.SetParent(root.transform);
            tokenSymbol.transform.localScale = new Vector3(0.25f, 0.05f, 0.25f);
            tokenSymbol.transform.localPosition = new Vector3(0, 0.08f, 0);
            DestroyImmediate(tokenSymbol.GetComponent<SphereCollider>());
            
            var symbolRenderer = tokenSymbol.GetComponent<MeshRenderer>();
            unitToken.SymbolRenderer = symbolRenderer;

            // 4. 创建 World Space Canvas
            var canvasObj = new GameObject("TokenCanvas");
            canvasObj.transform.SetParent(root.transform);
            canvasObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            canvasObj.transform.localRotation = Quaternion.Euler(30, 0, 0);
            
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            var rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(2, 1.5f);

            // 5. 创建UI文本
            // 番号
            var nameObj = CreateTextObject("Text_UnitName", canvasObj.transform, 
                new Vector3(0, 1.6f, 0), UnitName, 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            unitToken.Text_UnitName = nameObj.GetComponent<Text>();

            // 攻击力
            var atkObj = CreateTextObject("Text_ATK", canvasObj.transform,
                new Vector3(-0.6f, 1.4f, 0), $"ATK: {AttackPower}", 14, TextAnchor.MiddleLeft, FontStyle.Normal);
            unitToken.Text_ATK = atkObj.GetComponent<Text>();

            // 防御力
            var defObj = CreateTextObject("Text_DEF", canvasObj.transform,
                new Vector3(0.1f, 1.4f, 0), $"DEF: {DefensePower}", 14, TextAnchor.MiddleLeft, FontStyle.Normal);
            unitToken.Text_DEF = defObj.GetComponent<Text>();

            // 状态
            var stateObj = CreateTextObject("Text_State", canvasObj.transform,
                new Vector3(0, 1.0f, 0), "正常", 12, TextAnchor.MiddleCenter, FontStyle.Normal);
            unitToken.Text_State = stateObj.GetComponent<Text>();

            // 6. 创建兵力条
            var strengthBarObj = new GameObject("StrengthBar");
            strengthBarObj.transform.SetParent(canvasObj.transform);
            strengthBarObj.transform.localPosition = new Vector3(-0.7f, 1.2f, 0);
            
            var strengthTransforms = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                var barObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                barObj.name = $"Str_0{i + 1}";
                barObj.transform.SetParent(strengthBarObj.transform);
                barObj.transform.localPosition = new Vector3(-0.6f + i * 0.2f, 1.2f, 0);
                barObj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                barObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                DestroyImmediate(barObj.GetComponent<Collider>());
                
                strengthTransforms[i] = barObj.transform;
            }
            unitToken.StrengthBars = strengthTransforms;

            return unitToken;
        }

        /// <summary>
        /// 创建文本对象
        /// </summary>
        GameObject CreateTextObject(string name, Transform parent, Vector3 localPos, 
            string text, int fontSize, TextAnchor alignment, FontStyle style)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPos;
            
            var textComponent = obj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.fontStyle = style;
            textComponent.color = Color.white;
            
            // 使用默认字体
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            // 注意：Text组件会自动添加CanvasRenderer和RectTransform，不需要手动添加
            
            // 获取并设置RectTransform
            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(2, 0.3f);
            }
            
            return obj;
        }

        /// <summary>
        /// 初始化兵牌属性
        /// </summary>
        void InitializeUnitProperties(UnitToken unit)
        {
            unit.UnitId = UnitId;
            unit.UnitName = UnitName;
            unit.faction = faction;
            unit.tier = tier;
            unit.grade = grade;
            unit.AttackPower = AttackPower;
            unit.DefensePower = DefensePower;
            unit.MoveSpeedKmPerDay = MoveSpeedKmPerDay;
            unit.Firepower = Firepower;
            unit.Strength = Strength;
            unit.Morale = Morale;
            unit.Supply = Supply;
            unit.CurrentState = UnitState.Normal;
        }

        /// <summary>
        /// 查找第一个可用的地块
        /// </summary>
        HexTile FindFirstAvailableTile(HexGrid hexGrid)
        {
            foreach (var tile in hexGrid.AllTiles)
            {
                if (!tile.IsOccupied)
                {
                    return tile;
                }
            }
            return null;
        }

        /// <summary>
        /// 在指定坐标生成兵牌（重载）
        /// </summary>
        public UnitToken SpawnUnitAt(HexCoord coord)
        {
            spawnCoord = coord;
            return SpawnUnit();
        }
    }
}