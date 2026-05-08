using UnityEngine;
using TBS.Core.Events;
using TBS.Contracts.Events;
using TBS.Map.Tools;
using TBS.UnitSystem;

namespace TBS.Core
{
    /// <summary>
    /// 单位生成控制器 - 监听生成事件并创建单位
    /// </summary>
    public class SpawnController : MonoBehaviour
    {
        private UnitTokenSpawner spawner;

        private void Awake()
        {
            // 订阅单位生成事件
            EventBus.On<UnitSpawnRequestedEvent>(OnUnitSpawnRequested);
            Debug.Log("[SpawnController] 已初始化，正在监听生成事件");
        }

        private void OnUnitSpawnRequested(UnitSpawnRequestedEvent evt)
        {
            Debug.Log($"[SpawnController] 收到生成事件：{evt.Params.DisplayName} @ {evt.TargetCoord}");

            // 获取或创建 Spawner
            if (spawner == null)
            {
                spawner = FindObjectOfType<UnitTokenSpawner>();
                if (spawner == null)
                {
                    spawner = gameObject.AddComponent<UnitTokenSpawner>();
                    Debug.Log("[SpawnController] 动态创建了 UnitTokenSpawner");
                }
            }

            // 调用新的重载方法，传入 UnitRuntimeParams
            var token = spawner.SpawnUnit(evt.TargetCoord, evt.Params);
            if (token != null)
            {
                Debug.Log($"[SpawnController] 单位生成成功：{token.UnitName}");
                EventBus.Emit(new UnitSpawnedEvent { Token = token });
            }
            else
            {
                Debug.LogError("[SpawnController] 单位生成失败");
            }
        }

        private void OnDestroy()
        {
            EventBus.Off<UnitSpawnRequestedEvent>(OnUnitSpawnRequested);
            Debug.Log("[SpawnController] 已销毁");
        }
    }
}
