using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NoviceJob : Job {

    public NoviceJob() {
        Stats = new Stats();
        Skills = new List<Skill>();
        Skills.Add(new FirstAidSkill());
    }

    public override bool Evade(Character defender, Character attacker, Tile t)
    {
        throw new NotImplementedException();
    }
}
