using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TBS.Data.Config;

namespace TBS.Data
{
    /// <summary>
    /// 配置总管理器。挂载到场景中持久化 GameObject 上（单例）。
    /// 负责从 StreamingAssets/Config/ 加载所有 JSON 配置并提供查询接口。
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        public static ConfigManager Instance { get; private set; }

        // ── 原始集合 ──────────────────────────────────────────────────
        public UnitTokenCollection    UnitTokens    { get; private set; }
        public CombatWillCollection   CombatWills   { get; private set; }
        public WeatherCollection      Weathers      { get; private set; }
        public FortificationCollection Fortifications { get; private set; }
        public UnitStateCollection    UnitStates    { get; private set; }

        // ── 快查字典 ──────────────────────────────────────────────────
        private Dictionary<int,    UnitTokenData>    _unitById;
        private Dictionary<string, CombatWillData>   _willByGrade;
        private Dictionary<string, WeatherData>      _weatherByType;
        private Dictionary<int,    FortificationData> _fortByLevel;
        private Dictionary<string, UnitStateData>    _stateByKey;

        private const string CONFIG_DIR = "Config";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }

        // ── 加载入口 ─────────────────────────────────────────────────

        private void LoadAll()
        {
            UnitTokens     = Load<UnitTokenCollection>   ("unit_tokens.json");
            CombatWills    = Load<CombatWillCollection>  ("combat_will.json");
            Weathers       = Load<WeatherCollection>     ("weather.json");
            Fortifications = Load<FortificationCollection>("fortification.json");
            UnitStates     = Load<UnitStateCollection>   ("unit_states.json");

            BuildIndices();
            Debug.Log("[ConfigManager] 所有配置加载完成。");
        }

        private T Load<T>(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, CONFIG_DIR, fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[ConfigManager] 配置文件不存在：{path}");
                return default;
            }

            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            T result = JsonUtility.FromJson<T>(json);
            Debug.Log($"[ConfigManager] 已加载 {fileName}");
            return result;
        }

        private void BuildIndices()
        {
            _unitById = new Dictionary<int, UnitTokenData>();
            if (UnitTokens?.units != null)
                foreach (var u in UnitTokens.units)
                    _unitById[u.id] = u;

            _willByGrade = new Dictionary<string, CombatWillData>();
            if (CombatWills?.combatWills != null)
                foreach (var w in CombatWills.combatWills)
                    _willByGrade[w.grade] = w;

            _weatherByType = new Dictionary<string, WeatherData>();
            if (Weathers?.weatherTypes != null)
                foreach (var w in Weathers.weatherTypes)
                    _weatherByType[w.type] = w;

            _fortByLevel = new Dictionary<int, FortificationData>();
            if (Fortifications?.fortificationLevels != null)
                foreach (var f in Fortifications.fortificationLevels)
                    _fortByLevel[f.level] = f;

            _stateByKey = new Dictionary<string, UnitStateData>();
            if (UnitStates?.unitStates != null)
                foreach (var s in UnitStates.unitStates)
                    _stateByKey[s.state] = s;
        }

        // ── 查询接口 ─────────────────────────────────────────────────

        /// <summary>按序号获取兵牌数值。</summary>
        public UnitTokenData GetUnit(int id)
        {
            _unitById.TryGetValue(id, out var data);
            return data;
        }

        /// <summary>获取指定等级的作战意志配置。grade 如 "国军·精锐"</summary>
        public CombatWillData GetCombatWill(string grade)
        {
            _willByGrade.TryGetValue(grade, out var data);
            return data;
        }

        /// <summary>获取天气配置。type 为 "Sunny" 或 "Rainy"</summary>
        public WeatherData GetWeather(string type)
        {
            _weatherByType.TryGetValue(type, out var data);
            return data;
        }

        /// <summary>获取工事等级配置（0-4）。</summary>
        public FortificationData GetFortification(int level)
        {
            _fortByLevel.TryGetValue(level, out var data);
            return data;
        }

        /// <summary>获取状态效果配置。state 如 "Routed"</summary>
        public UnitStateData GetUnitState(string state)
        {
            _stateByKey.TryGetValue(state, out var data);
            return data;
        }

        /// <summary>返回所有兵牌数值列表。</summary>
        public IReadOnlyList<UnitTokenData> GetAllUnits()
            => UnitTokens?.units ?? Array.Empty<UnitTokenData>();
    }
}
