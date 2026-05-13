using UnityEngine;
using TBS.Core.Events;
using TBS.Contracts.Events;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using TBS.Map.Managers;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.SpawnPanel
{
    public class SpawnPanelPresenter : BasePresenter<SpawnPanelView, SpawnPanelViewModel>
    {
        private UnityEngine.Camera gameCamera;
        private MapManager mapManager;

        protected override void OnInitialize()
        {
            Debug.Log("[SpawnPanelPresenter] OnInitialize");

            // 获取摄像机和地图管理器
            gameCamera = UnityEngine.Camera.main;
            mapManager = MapManager.Instance;
            if (mapManager == null)
                mapManager = Object.FindObjectOfType<MapManager>();

            // 订阅鼠标点击事件
            EventBus.On<MouseButtonDownEvent>(OnMouseButtonDown);

            Debug.Log($"[SpawnPanelPresenter] 初始化完成，Camera: {(gameCamera != null ? "有" : "无")}, MapManager: {(mapManager != null ? "有" : "无")}");
        }

        private void OnMouseButtonDown(MouseButtonDownEvent evt)
        {
            // 若无选中单位或点击在UI上，则忽略
            if (View?.ViewModel.SelectedEntry == null || evt.IsOverUI)
                return;

            // 只处理左键
            if (evt.Button != 0)
                return;

            Debug.Log("[SpawnPanelPresenter] 左键点击，尝试在地图上生成单位");

            // 做射线检测找到HexTile
            if (gameCamera == null)
            {
                Debug.LogError("[SpawnPanelPresenter] 摄像机为空");
                return;
            }

            Ray ray = gameCamera.ScreenPointToRay(evt.ScreenPos);
            RaycastHit hit;

            if (mapManager == null)
            {
                mapManager = MapManager.Instance;
                if (mapManager == null)
                    mapManager = Object.FindObjectOfType<MapManager>();
                if (mapManager == null)
                {
                    Debug.LogError("[SpawnPanelPresenter] MapManager为空");
                    return;
                }
            }

            // 检测是否命中地块
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                var tile = hit.collider.GetComponent<MapTileCell>()
                    ?? hit.collider.GetComponentInParent<MapTileCell>();
                if (tile != null && !tile.IsOccupied)
                {
                    Debug.Log($"[SpawnPanelPresenter] 点击了空闲地块 {tile.Coord}，发送生成事件");

                    // 发送生成事件
                    var spawnEvent = new UnitSpawnRequestedEvent
                    {
                        TargetCoord = tile.Coord,
                        Params = View.ViewModel.SelectedEntry.Params
                    };
                    EventBus.Emit(spawnEvent);

                    // 从列表中移除该单位
                    View.ViewModel.RemoveSelectedUnit();
                    Debug.Log("[SpawnPanelPresenter] 单位已生成并从列表中移除");
                }
                else if (tile != null && tile.IsOccupied)
                {
                    Debug.LogWarning($"[SpawnPanelPresenter] 地块 {tile.Coord} 已被占据");
                }
                else
                {
                    Debug.LogWarning("[SpawnPanelPresenter] 点击位置不是地块");
                }
            }
            else
            {
                Debug.LogWarning("[SpawnPanelPresenter] 射线未命中任何物体");
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            Debug.Log("[SpawnPanelPresenter] SpawnPanel 显示");
        }

        public override void OnHide()
        {
            base.OnHide();
            Debug.Log("[SpawnPanelPresenter] SpawnPanel 隐藏");
        }

        public override void OnDestroy()
        {
            EventBus.Off<MouseButtonDownEvent>(OnMouseButtonDown);
            base.OnDestroy();
            Debug.Log("[SpawnPanelPresenter] 已销毁");
        }
    }
}
