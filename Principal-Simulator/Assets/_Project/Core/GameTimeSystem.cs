using UnityEngine;

namespace TBS.Core
{
    /// <summary>
    /// 游戏时间系统 — 将真实秒数映射为游戏小时
    /// 一格约等于10公里，正规步兵一天行军速度约25km/day = 2.5格/天
    /// realSecondsPerGameDay 控制时间流速
    /// </summary>
    public class GameTimeSystem : MonoBehaviour
    {
        public static GameTimeSystem Instance { get; private set; }

        [Header("时间设置")]
        [Tooltip("现实中多少秒 = 游戏1天（24小时）")]
        [SerializeField] private float realSecondsPerGameDay = 60f;

        [Tooltip("每格地图代表的公里数")]
        [SerializeField] private float kmPerHexTile = 10f;

        // 当前游戏时间（以小时计）
        private float gameHours;

        // 上一次 Tick 时的游戏小时（整数）
        private int lastTickHour = -1;

        public float GameHours => gameHours;
        public int GameDay => Mathf.FloorToInt(gameHours / 24);
        public float HoursPerRealSecond => 24f / realSecondsPerGameDay;
        public float KmPerHexTile => kmPerHexTile;

        /// <summary>
        /// 单位从一格走到相邻格需要的真实秒数
        /// </summary>
        public float GetRealSecondsPerHex(float moveSpeedKmPerDay)
        {
            if (moveSpeedKmPerDay <= 0f) return float.MaxValue;
            // 多少游戏小时走一格
            float gameHoursPerHex = (kmPerHexTile / moveSpeedKmPerDay) * 24f;
            // 换算成真实秒数
            return gameHoursPerHex * (realSecondsPerGameDay / 24f);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            gameHours += Time.deltaTime * HoursPerRealSecond;

            int currentHour = Mathf.FloorToInt(gameHours);
            if (currentHour != lastTickHour)
            {
                lastTickHour = currentHour;
                OnHourTick(currentHour);
            }
        }

        private void OnHourTick(int hour)
        {
            // 可扩展：在此广播事件给 Unit.Tick()
        }
    }
}
