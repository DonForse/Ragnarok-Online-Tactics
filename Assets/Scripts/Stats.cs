using UnityEngine;
using System.Collections;

public class Stats
{
    //Amount of Movement Points per turn.
    public int Movement {get;set;}

    /// <summary>
    /// Level of the Character
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Total Experience until next level.
    /// </summary>
    public int Exp { get; set; }

    /// <summary>
    /// Total Amount of Hit Points.
    /// </summary>
    public int TotalHP { get; set; }

    private int hp { get; set; }
    /// <summary>
    /// Current amount of Hit Points.
    /// </summary>
    public int HP {
        get { return hp; }
        set { hp = value > TotalHP ? TotalHP : value < 0 ? 0 : value; }
    }

    /// <summary>
    /// Used in skills.
    /// </summary>
    public int MP { get; set; }

    /// <summary>
    /// Increace damage on short range units.
    /// </summary>
    public int Str { get; set; }
    
    /// <summary>
    /// Increase chance to hit, Increase damage on ranged units, Increace chance to evade by little.
    /// </summary>
    public int Dex { get; set; }

    /// <summary>
    /// Increase AA ammount and decrease AA received amount, Increase chance to evade.
    /// </summary>
    public int Agi { get; set; }

    /// <summary>
    /// Increase Magic Damage and MaxMp.
    /// </summary>
    public int Int { get; set; }
    
    /// <summary>
    /// Increase CritChance ammount by n %.
    /// </summary>
    public int Lck { get; set; }
}
