using UnityEngine;
using System.Collections;

public static class BattleHelper
{
    public static int CalculateDamage(Character attacker, Character target, Tile targetTile)
    {
        //TODO: Next Iteration (no demo).
        /*
         * o int para magias
         Strength + (Weapon Might + Weapon Triangle Bonus) X Weapon effectiveness + Support Bonus
         */
        if (HasHit(attacker, target, targetTile))
        {
            return (attacker.Stats.Str + 1 * attacker.Stats.Dex / 2) * 1;//CalculateAttackTimes(attacker, target);
        }
        return 0;
        
    }
    public static int CalculateAttackTimes(Character attacker, Character target) {
        return attacker.Stats.Agi - target.Stats.Agi > 0 ? 2 : 1;
        /*
            If WWt ≤ Con, then AS = Speed
            If WWt > Con, then AS = Speed - (Weapon Weight - Con)
         */
    }
    public static bool HasHit(Character attacker, Character target, Tile targetTile)
    {
        var chance = 100 - (target.Stats.Agi * 4 + targetTile.AvoidValue) + target.Stats.Dex * 3;
        var rand = Random.Range(0, 100);

        return rand <= chance;
        /*
          Hit Rate = Weapon Accuracy + Skill x 2 + Luck / 2
                + Support Bonus + S-Rank Bonus + Tactician Bonus

         * Evade = Attack Speed x 2 + Luck + Terrain Bonus
         + Support Bonus + Tactician Bonus
         * 
         * Accuracy = Hit Rate (Attacker) - Evade (Defender) + Triangle Bonus
         * 
         */
    }
}
