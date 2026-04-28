namespace TBS.Unit
{
    /// <summary>
    /// 工事构筑系统 — 实现 IFortificationBuilder，由时间系统驱动
    /// 逻辑已内嵌在 Unit.TickFortification；此类提供炮击破坏入口
    /// </summary>
    public class FortificationSystem : IFortificationBuilder
    {
        public void Tick(IUnitToken unit, float deltaHours, WeatherType weather)
        {
            if (unit is Unit u) u.Tick(deltaHours, weather, false);
        }

        public int GetCurrentLevel(IUnitToken unit) => unit.FortificationLevel;

        /// <summary>
        /// 炮击破坏工事 — artilleryGrade≥3 时工事-1（文档§7.2）
        /// </summary>
        public void Damage(IUnitToken unit, int artilleryGrade)
        {
            if (unit is not Unit u) return;
            if (artilleryGrade < 3) return;

            // 通过反射-like 方式降低等级（字段 private，用公开方法暴露）
            u.DamageFortification();
        }
    }
}
