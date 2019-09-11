public enum Faction
{
    /* Own Team */
    Cultist = 1,

    /* Enemy Teams */
    DawnOfVictory = 2,
    Aggressor = 3, 
    DescendantsOfFianna = 4,
    SOSBrigade = 5
}

public enum MenuActionEvent
{
    Attack,
    Items,
    Skills,
    Wait,
    Back
}

public enum Action { 
    Move,
    Attack,
    Skill,
    None
}

public enum TileType 
{
    Mountain,
    Peak,
    Forest,
    Cliff,
    Home,
    Gate,
    Plain,
    Castle,
}

public enum StatusAI
{ 
    None = 0,
    /// <summary>
    /// Status for the enemy, will defend his current position, not moving, but attacking enemies in Range.
    /// </summary>
    Defensive = 1,
    /// <summary>
    /// Status for the enemy, will try to reach closest enemy and attack.
    /// </summary>
    Aggressive = 2,
}

public enum IdSkill { 
    FirstAid = 1,
    Test = 2
}
public enum SkillType { 
    Heal = 1,
    Offensive = 2,
}
