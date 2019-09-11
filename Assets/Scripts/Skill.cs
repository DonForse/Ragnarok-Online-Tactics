using UnityEngine;
using System.Collections;

public abstract class Skill {

    public IdSkill Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Range { get; set; }
    public SkillType Type { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <returns>damage</returns>
    public abstract int Execute(Character user, Vector2 target);

}
