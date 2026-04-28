using TBS.Contracts.Combat;
using TBS.Unit;
using UnityEngine;

namespace TBS.Gameplay.Combat
{
    public sealed class DamageCalculator : IDamageCalculator
    {
        const float DamageThreshold    = 3.0f;
        const float CounterAttackRate  = 0.6f;
        const float ArtilleryModifier  = 1.0f;
        const float FortShieldPerLevel = 0.08f;
        const float ReactionFireMod    = 0.3f;

        public CombatDamage CalculateAssault(CombatParameters p)
        {
            float effectiveAttack  = (p.Attacker.AttackPower + p.AttackBonusFlat)
                                   * p.SupplyAttackModifier
                                   * p.TerrainAttackModifier
                                   * p.WeatherModifier
                                   * p.CombinedAttackBonus;

            float fortBonus = p.IgnoreFortification ? 0 : p.FortificationLevel;
            float effectiveDefense = (p.Defender.DefensePower + fortBonus + p.TerrainDefenseBonus)
                                   * p.SupplyDefenseModifier;

            float rawToDefender = Mathf.Max(0, effectiveAttack - effectiveDefense);
            float rawToAttacker = Mathf.Max(0, effectiveDefense * CounterAttackRate - effectiveAttack * 0.5f);

            int strLossDefender = Round(rawToDefender / DamageThreshold);
            int strLossAttacker = Round(rawToAttacker / DamageThreshold);

            var damage = new CombatDamage
            {
                DefenderStrengthLoss = strLossDefender,
                AttackerStrengthLoss = strLossAttacker,
            };

            damage.DefenderMoraleLoss += strLossDefender * 6;
            damage.AttackerMoraleLoss += strLossAttacker * 6;

            if (strLossDefender > strLossAttacker)
            {
                damage.AttackerMoraleLoss -= 5; // attacker morale gain
                damage.DefenderMoraleLoss += 8;
            }
            else if (strLossAttacker > strLossDefender)
            {
                damage.AttackerMoraleLoss += 8;
                damage.DefenderMoraleLoss -= 5; // defender morale gain
            }

            return damage;
        }

        public int CalculateArtillery(IUnitToken source, IUnitToken target, IGameContext context)
        {
            float weatherAccuracy = context.CurrentWeather == WeatherType.Rain ? 0.7f : 1.0f;
            float fortShield = 1f - target.FortificationLevel * FortShieldPerLevel;
            float damage = source.Firepower * ArtilleryModifier * weatherAccuracy * fortShield;
            return Mathf.FloorToInt(damage);
        }

        public int CalculateReactionFire(IUnitToken source, IUnitToken target)
        {
            float supply = source.Supply >= 3 ? 1.0f : source.Supply >= 1 ? 0.8f : 0.5f;
            return Round(source.AttackPower * ReactionFireMod * supply);
        }

        static int Round(float value) => Mathf.RoundToInt(value);
    }
}
