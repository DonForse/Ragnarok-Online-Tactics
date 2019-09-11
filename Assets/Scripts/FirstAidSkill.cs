using UnityEngine;
using System.Collections;

public class FirstAidSkill : Skill 
{

    public FirstAidSkill()
    {
        Id = IdSkill.FirstAid;
        Name = "First Aid";
        Description = "Basic skills fundamental to any novice adventurer.";
        Range = 0;
        Type = SkillType.Heal;
    }

    public override int Execute(Character user, Vector2 target)
    {
        var healingPower = Random.Range(7, 10);
        user.Stats.HP += healingPower;
        return -1 * healingPower; // -1 xq es una curacion.
    }
}
