using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Job : MonoBehaviour {
    public int SkillCount;
    internal IList<Skill> Skills;
    internal Stats Stats { get; set; }

    public abstract bool Evade(Character defender, Character attacker, Tile t);
}
