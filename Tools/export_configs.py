"""
export_configs.py
把 .cursor/02-系统级文档/单位系统/兵牌数值表.xlsx 导出为
Assets/StreamingAssets/Config/ 下的 JSON 配置文件。

用法：在项目根目录执行
    python Tools/export_configs.py
"""

import json
import pathlib
import openpyxl

ROOT      = pathlib.Path(__file__).parent.parent
XLSX_PATH = ROOT / ".cursor/02-系统级文档/单位系统/兵牌数值表.xlsx"
OUT_DIR   = ROOT / "Assets/StreamingAssets/Config"
OUT_DIR.mkdir(parents=True, exist_ok=True)

wb = openpyxl.load_workbook(XLSX_PATH, data_only=True)

# ── 1. 兵牌数值 ──────────────────────────────────────────────────
def export_units():
    ws = wb["兵牌数值总表"]
    rows = list(ws.iter_rows(min_row=2, values_only=True))

    headers = [h.split("\n")[0] if h else "" for h in rows[0]]
    COL = {h: i for i, h in enumerate(headers)}

    units = []
    for row in rows[1:]:
        seq = row[COL["序号"]]
        if not isinstance(seq, int):     # 分节标题行 / 注释行
            continue
        units.append({
            "id":          seq,
            "name":        row[COL["兵牌名称"]],
            "grade":       row[COL["势力/等级"]],
            "attack":      row[COL["攻击"]],
            "defense":     row[COL["防御"]],
            "moveSpeed":   row[COL["行军速度"]],
            "firepower":   row[COL["火力"]],
            "initMorale":  row[COL["初始士气"]],
            "initSupply":  row[COL["初始补给"]],
            "initStrength":row[COL["兵力值"]],
        })

    out = {"units": units}
    path = OUT_DIR / "unit_tokens.json"
    path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[OK] unit_tokens.json  ({len(units)} 条)")

# ── 2. 作战意志 ──────────────────────────────────────────────────
def export_combat_will():
    ws = wb["状态系统速查"]

    # 作战意志表固定在文档中，按等级名硬编码（表格结构较复杂不适合动态解析）
    will_data = [
        {"grade":"国军·精锐", "shakenThresholdMorale":40, "routedCondition":"morale<=30 AND strength<=1 OR morale==0", "moraleRecoveryPerHour":2.5, "replenishSpeedMod":1.0},
        {"grade":"国军·正规", "shakenThresholdMorale":45, "routedCondition":"strength<=1 OR morale<=15",               "moraleRecoveryPerHour":1.5, "replenishSpeedMod":0.8},
        {"grade":"国军·杂牌", "shakenThresholdMorale":50, "routedCondition":"strength<=2 OR morale<=25",               "moraleRecoveryPerHour":0.5, "replenishSpeedMod":0.5},
        {"grade":"日军",      "shakenThresholdMorale":35, "routedCondition":"morale<=20 AND strength<=1 OR morale==0", "moraleRecoveryPerHour":3.0, "replenishSpeedMod":1.2},
        {"grade":"八路军·精锐","shakenThresholdMorale":40,"routedCondition":"strength<=1 OR morale<=10",               "moraleRecoveryPerHour":2.0, "replenishSpeedMod":1.0},
        {"grade":"八路军·普通","shakenThresholdMorale":45,"routedCondition":"strength<=1 OR morale<=20",               "moraleRecoveryPerHour":1.5, "replenishSpeedMod":0.8},
    ]
    out = {"combatWills": will_data}
    path = OUT_DIR / "combat_will.json"
    path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[OK] combat_will.json  ({len(will_data)} 条)")

# ── 3. 天气配置 ──────────────────────────────────────────────────
def export_weather():
    weather_data = [
        {"type":"Sunny", "displayName":"晴天", "moveSpeedMod":1.0, "airSupportAvailable":True,  "artilleryAccuracyMod":1.0,  "fortBuildSpeedMod":1.0, "moralePerDay":0},
        {"type":"Rainy", "displayName":"下雨", "moveSpeedMod":0.5, "airSupportAvailable":False, "artilleryAccuracyMod":0.7,  "fortBuildSpeedMod":0.5, "moralePerDay":-2},
    ]
    out = {"weatherTypes": weather_data}
    path = OUT_DIR / "weather.json"
    path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[OK] weather.json      ({len(weather_data)} 条)")

# ── 4. 工事配置 ──────────────────────────────────────────────────
def export_fortification():
    fort_data = [
        {"level":0, "name":"无工事",   "defenseBonus":0, "buildTimeDays":0, "suppressionHitsRequired":1},
        {"level":1, "name":"散兵坑",   "defenseBonus":1, "buildTimeDays":1, "suppressionHitsRequired":1},
        {"level":2, "name":"野战壕",   "defenseBonus":2, "buildTimeDays":1, "suppressionHitsRequired":2},
        {"level":3, "name":"加固阵地", "defenseBonus":3, "buildTimeDays":1, "suppressionHitsRequired":3},
        {"level":4, "name":"永备工事", "defenseBonus":4, "buildTimeDays":1, "suppressionHitsRequired":4},
    ]
    out = {"fortificationLevels": fort_data}
    path = OUT_DIR / "fortification.json"
    path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[OK] fortification.json({len(fort_data)} 条)")

# ── 5. 状态效果配置 ───────────────────────────────────────────────
def export_unit_states():
    state_data = [
        {"state":"Inspired",     "displayName":"高昂",   "attackMod":2,  "defenseMod":0, "moveSpeedMod":1.2, "canAttack":True,  "canCommand":True,  "canReplenish":False},
        {"state":"Normal",       "displayName":"正常",   "attackMod":0,  "defenseMod":0, "moveSpeedMod":1.0, "canAttack":True,  "canCommand":True,  "canReplenish":False},
        {"state":"Suppressed",   "displayName":"压制",   "attackMod":0,  "defenseMod":0, "moveSpeedMod":0.5, "canAttack":False, "canCommand":False, "canReplenish":False},
        {"state":"Shaken",       "displayName":"动摇",   "attackMod":-1, "defenseMod":-1,"moveSpeedMod":0.8, "canAttack":True,  "canCommand":False, "canReplenish":False},
        {"state":"Routed",       "displayName":"溃散",   "attackMod":0,  "defenseMod":-2,"moveSpeedMod":0.7, "canAttack":False, "canCommand":False, "canReplenish":False},
        {"state":"Recuperating", "displayName":"后撤整补","attackMod":0,  "defenseMod":0, "moveSpeedMod":1.0, "canAttack":False, "canCommand":True,  "canReplenish":True},
    ]
    out = {"unitStates": state_data}
    path = OUT_DIR / "unit_states.json"
    path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[OK] unit_states.json  ({len(state_data)} 条)")

if __name__ == "__main__":
    print(f"读取: {XLSX_PATH}")
    print(f"输出: {OUT_DIR}\n")
    export_units()
    export_combat_will()
    export_weather()
    export_fortification()
    export_unit_states()
    print("\n全部导出完成。")
