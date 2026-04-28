using UnityEngine;
using UnityEngine.UI;
using TBS.Map.Tools;
using TBS.Map.Components;
using TBS.Map.API;

namespace TBS.UnitSystem
{
    /// <summary>
    /// 兵牌核心组件 - 代表团级作战单位
    /// 历史背景：淞沪会战（1937年8-11月）
    /// </summary>
    public class UnitToken : MonoBehaviour, IUnit
    {
        [Header("地图坐标")]
        [SerializeField] private HexCoord currentCoord;
        public HexCoord CurrentCoord 
        { 
            get => currentCoord;
            set 
            {
                currentCoord = value;
                // 更新世界位置
                UpdatePositionFromCoord();
            }
        }

        [Header("基础识别")]
        [SerializeField] private string _unitId;
        [SerializeField] private string _unitName;
        
        public string UnitId 
        { 
            get => _unitId; 
            set => _unitId = value; 
        }
        
        public Faction faction;
        public UnitTier tier;
        public UnitGrade grade;
        
        public string UnitName 
        { 
            get => _unitName; 
            set => _unitName = value; 
        }

        [Header("战斗属性")]
        [Range(0, 10)]
        public int AttackPower = 5;
        [Range(0, 10)]
        public int DefensePower = 5;
        public float MoveSpeedKmPerDay = 25f;
        [Range(0, 10)]
        public int Firepower = 3;

        [Header("资源属性")]
        [Range(0, 5)]
        public int Strength = 5;  // 兵力值（满5=满编）
        [Range(0, 100)]
        public int Morale = 80;   // 士气
        [Range(0, 5)]
        public int Supply = 4;    // 补给

        [Header("状态")]
        public UnitState CurrentState = UnitState.Normal;

        [Header("工事")]
        [Range(0, 4)]
        public int FortificationLevel = 0;
        [Range(0f, 1f)]
        public float FortificationProgress = 0f;

        [Header("UI引用")]
        public Text Text_ATK;
        public Text Text_DEF;
        public Text Text_UnitName;
        public Text Text_State;
        public Transform[] StrengthBars; // 5格兵力条
        public Renderer BorderRenderer; // 势力色边框
        public Renderer BaseRenderer;   // 底座渲染器
        public Renderer SymbolRenderer; // 标识符号

        [Header("选中效果")]
        public GameObject SelectionIndicator; // 选中指示器（如光环）
        public Color SelectedColor = Color.yellow;
        public float SelectedGlowIntensity = 2f;

        // 内部状态
        private bool isSelected = false;
        private Color originalBorderColor;
        private HexGrid hexGrid;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        // 势力配色
        public static readonly Color Color_Nationalist_Elite = new Color(0, 0, 0.5f);  // 深蓝
        public static readonly Color Color_Nationalist_Normal = new Color(0, 0.3f, 0.8f); // 蓝色
        public static readonly Color Color_Japanese = new Color(0.5f, 0, 0);           // 深红
        public static readonly Color Color_EighthRoute = new Color(0.8f, 0, 0);        // 红色
        public static readonly Color Color_Gold = new Color(1, 0.84f, 0);              // 金色

        // 事件
        public System.Action<UnitToken> OnUnitSelected;
        public System.Action<UnitToken> OnUnitDeselected;
        public System.Action<HexCoord> OnUnitMoved;

        void Start()
        {
            hexGrid = FindObjectOfType<HexGrid>();
            UpdateVisuals();
            
            // 保存原始边框颜色
            if (BorderRenderer != null)
            {
                originalBorderColor = BorderRenderer.material.color;
            }

            // 确保有碰撞器用于点击检测
            EnsureClickable();
        }

        void Update()
        {
            // 持续面向相机
            if (Camera.main != null && transform.GetChild(1) != null)
            {
                transform.GetChild(1).rotation = Camera.main.transform.rotation;
            }
        }

        /// <summary>
        /// 确保兵牌可以被点击
        /// </summary>
        void EnsureClickable()
        {
            // 检查是否有碰撞器
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                // 在底座添加碰撞器
                var baseObj = transform.Find("TokenBase");
                if (baseObj != null)
                {
                    var baseCollider = baseObj.GetComponent<Collider>();
                    if (baseCollider == null)
                    {
                        baseObj.gameObject.AddComponent<BoxCollider>();
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标点击时触发（需要碰撞器）
        /// </summary>
        void OnMouseDown()
        {
            // 通知选择管理器选中此单位
            var selectionManager = FindObjectOfType<UnitSelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.SelectUnit(this);
            }
            else
            {
                // 如果没有选择管理器，直接切换选中状态
                ToggleSelection();
            }
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            // 更新视觉反馈
            UpdateSelectionVisual();

            if (isSelected)
            {
                OnUnitSelected?.Invoke(this);
            }
            else
            {
                OnUnitDeselected?.Invoke(this);
            }
        }

        /// <summary>
        /// 切换选中状态
        /// </summary>
        public void ToggleSelection()
        {
            SetSelected(!isSelected);
        }

        /// <summary>
        /// 更新选中视觉效果
        /// </summary>
        void UpdateSelectionVisual()
        {
            // 选中指示器 - 创建/显示光环
            EnsureSelectionIndicator();
            if (SelectionIndicator != null)
            {
                SelectionIndicator.SetActive(isSelected);
            }

            // 边框颜色变化
            if (BorderRenderer != null)
            {
                if (isSelected)
                {
                    BorderRenderer.material.color = SelectedColor; // 选中时变黄
                }
                else
                {
                    BorderRenderer.material.color = originalBorderColor; // 恢复原始颜色
                }
            }
            
            // 轻微放大效果
            Vector3 targetScale = isSelected ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.3f);
        }
        
        /// <summary>
        /// 确保选中指示器存在
        /// </summary>
        void EnsureSelectionIndicator()
        {
            if (SelectionIndicator == null)
            {
                // 创建一个简单的选中光环
                var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                indicator.name = "SelectionIndicator";
                indicator.transform.SetParent(transform);
                indicator.transform.localPosition = new Vector3(0, -0.02f, 0);
                indicator.transform.localScale = new Vector3(1.1f, 0.01f, 1.1f);
                
                DestroyImmediate(indicator.GetComponent<Collider>());
                
                var renderer = indicator.GetComponent<MeshRenderer>();
                renderer.material.color = Color.yellow;
                
                SelectionIndicator = indicator;
            }
        }

        /// <summary>
        /// 移动兵牌到指定坐标
        /// </summary>
        public void MoveTo(HexCoord targetCoord)
        {
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            if (hexGrid == null)
            {
                Debug.LogError("UnitToken: 无法找到HexGrid");
                return;
            }

            // 获取目标地块
            var targetTile = hexGrid.GetTile(targetCoord);
            if (targetTile == null)
            {
                Debug.LogWarning($"UnitToken: 目标坐标 {targetCoord} 不存在地块");
                return;
            }

            // 检查地块是否可进入
            if (!targetTile.CanEnter(this))
            {
                Debug.LogWarning($"UnitToken: 目标坐标 {targetCoord} 无法进入");
                return;
            }

            // 获取当前地块并清除占据
            var currentTile = hexGrid.GetTile(currentCoord);
            if (currentTile != null)
            {
                currentTile.ClearOccupyingUnit();
            }

            // 更新坐标
            currentCoord = targetCoord;
            
            // 更新世界位置
            UpdatePositionFromCoord();

            // 设置新地块的占据单位
            targetTile.SetOccupyingUnit(this);

            // 触发移动事件
            OnUnitMoved?.Invoke(targetCoord);

            Debug.Log($"UnitToken: {UnitName} 移动到 {targetCoord}");
        }

        /// <summary>
        /// 根据当前坐标更新世界位置
        /// </summary>
        void UpdatePositionFromCoord()
        {
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            if (hexGrid != null)
            {
                Vector3 worldPos = hexGrid.CoordToWorldPosition(currentCoord);
                // 保持Y高度不变（放在地形上方）
                worldPos.y = transform.position.y;
                transform.position = worldPos;
            }
        }

        /// <summary>
        /// 检查是否可以移动到指定坐标
        /// </summary>
        public bool CanMoveTo(HexCoord targetCoord)
        {
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            if (hexGrid == null) return false;

            // 检查距离（假设每次只能移动1格）
            int distance = currentCoord.DistanceTo(targetCoord);
            if (distance != 1)
            {
                return false;
            }

            // 检查目标地块
            var targetTile = hexGrid.GetTile(targetCoord);
            if (targetTile == null) return false;

            return targetTile.CanEnter(this);
        }

        /// <summary>
        /// 获取可移动的邻接坐标
        /// </summary>
        public HexCoord[] GetValidMoveTargets()
        {
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            if (hexGrid == null) return new HexCoord[0];

            var neighbors = currentCoord.GetNeighbors();
            var validTargets = new System.Collections.Generic.List<HexCoord>();

            foreach (var neighbor in neighbors)
            {
                var tile = hexGrid.GetTile(neighbor);
                if (tile != null && tile.CanEnter(this))
                {
                    validTargets.Add(neighbor);
                }
            }

            return validTargets.ToArray();
        }

        /// <summary>
        /// 初始化兵牌到指定坐标
        /// </summary>
        public void InitializeOnTile(HexCoord coord)
        {
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            currentCoord = coord;
            UpdatePositionFromCoord();

            // 设置地块占据
            var tile = hexGrid?.GetTile(coord);
            if (tile != null)
            {
                tile.SetOccupyingUnit(this);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// 更新兵牌视觉表现
        /// </summary>
        public void UpdateVisuals()
        {
            // 更新UI文本
            if (Text_ATK != null) Text_ATK.text = $"ATK: {AttackPower}";
            if (Text_DEF != null) Text_DEF.text = $"DEF: {DefensePower + FortificationLevel}";
            if (Text_UnitName != null) Text_UnitName.text = UnitName;
            if (Text_State != null) Text_State.text = GetStateDisplayName();

            // 更新兵力条
            UpdateStrengthBars();

            // 更新势力色
            UpdateFactionColors();
        }

        void UpdateStrengthBars()
        {
            if (StrengthBars == null || StrengthBars.Length == 0) return;

            for (int i = 0; i < StrengthBars.Length; i++)
            {
                if (StrengthBars[i] != null)
                {
                    var renderer = StrengthBars[i].GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // 满格显示绿色，空格显示灰色
                        renderer.material.color = i < Strength ? Color.green : Color.gray;
                        renderer.enabled = true;
                    }
                }
            }
        }

        void UpdateFactionColors()
        {
            Color borderColor;
            Color baseColor;

            switch (faction)
            {
                case Faction.Nationalist:
                    borderColor = (grade == UnitGrade.Elite) ? Color_Nationalist_Elite : Color_Nationalist_Normal;
                    baseColor = Color.white;
                    break;
                case Faction.Japanese:
                    borderColor = Color_Japanese;
                    baseColor = Color.white;
                    break;
                case Faction.EighthRoute:
                    borderColor = Color_EighthRoute;
                    baseColor = Color_Gold;
                    break;
                default:
                    borderColor = Color.gray;
                    baseColor = Color.white;
                    break;
            }

            originalBorderColor = borderColor;

            if (BorderRenderer != null) BorderRenderer.material.color = borderColor;
            if (BaseRenderer != null) BaseRenderer.material.color = baseColor;
            if (SymbolRenderer != null) SymbolRenderer.material.color = Color.black;
        }

        string GetStateDisplayName()
        {
            return CurrentState switch
            {
                UnitState.Inspired => "高昂",
                UnitState.Normal => "正常",
                UnitState.Suppressed => "压制",
                UnitState.Shaken => "动摇",
                UnitState.Routed => "溃散",
                UnitState.Recuperating => "整补",
                _ => "未知"
            };
        }

        /// <summary>
        /// 是否在选中状态
        /// </summary>
        public bool IsSelected => isSelected;

        #region IUnit Implementation

        /// <summary>
        /// 应用地形移动消耗修正
        /// </summary>
        public float ApplyTerrainMovementModifier(float baseCost, string terrainId)
        {
            // 基础实现，可根据单位特性扩展
            // 例如：八路军在山地移动更快，日军在道路移动更快
            return baseCost;
        }

        /// <summary>
        /// 检查是否可以通过特定地形
        /// </summary>
        public bool CanTraverseTerrain(string terrainId)
        {
            // 基础实现，所有地形都可通过
            // 可根据单位类型限制（如坦克不能通过山地）
            return true;
        }

        #endregion
    }

    /// <summary>
    /// 势力类型
    /// </summary>
    public enum Faction
    {
        Nationalist,  // 国军
        Japanese,     // 日军
        EighthRoute   // 八路军
    }

    /// <summary>
    /// 单位等级
    /// </summary>
    public enum UnitTier
    {
        HQ,           // 师部/司令部
        Regiment,     // 团
        Battalion     // 营
    }

    /// <summary>
    /// 单位等级（精锐/正规/杂牌）
    /// </summary>
    public enum UnitGrade
    {
        Elite,    // 精锐
        Normal,   // 正规
        Irregular // 杂牌/游击
    }

    /// <summary>
    /// 兵牌状态
    /// </summary>
    public enum UnitState
    {
        Inspired,     // 高昂：士气≥80 且 兵力≥4
        Normal,       // 正常
        Suppressed,   // 压制
        Shaken,       // 动摇
        Routed,       // 溃散
        Recuperating  // 后撤整补
    }
}