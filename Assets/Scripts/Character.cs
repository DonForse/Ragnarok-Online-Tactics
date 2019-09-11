using System;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class Character : MonoBehaviour
{
    internal Faction Team;
    internal string Name;
    internal Stats Stats { get; set; }
    internal Vector2 Coordinate;
    internal Job Job;
    internal Weapon Weapon;
    internal bool HasMoved;
    internal bool HasFinished;

    internal bool IsMoving { get; set; }
    internal bool IsAttacking{ get; set; }
    internal StatusAI AI;    

    internal Animator animator;
    internal Vector2 lastCoordinate;

    internal int movingTowardIndex;
    internal Vector2 movingTowardCoordinate;
    internal List<Vector2> movingTowardCoordinatePath;

    public delegate void OnCharacterMoved(CharacterMovedEventArgs e);
    public static event OnCharacterMoved CharacterMoved;
    public class CharacterMovedEventArgs : EventArgs
    {
        public Character character { get; set; }
    }

    public delegate void OnCharacterAttacked(CharacterAttackedEventArgs e);
    public static event OnCharacterAttacked CharacterAttacked;
    public class CharacterAttackedEventArgs : EventArgs
    {
        public Character character { get; set; }
    }

    public delegate void OnCharacterUsedSkill(CharacterUsedSkillEventArgs e);
    public static event OnCharacterUsedSkill CharacterUsedSkill;
    public class CharacterUsedSkillEventArgs : EventArgs
    {
        public Character character { get; set; }
    }

    public delegate void OnCharacterGotHit (CharacterGotHitEventArgs e);
    public static event OnCharacterGotHit CharacterGotHit;
    public class CharacterGotHitEventArgs : EventArgs
    {
        public Character character { get; set; }
        public int damage { get; set; }
    }
    
    public void ParseCharacter(XmlNode xCharacter) 
    {
        Team = (Faction)int.Parse(xCharacter.SelectSingleNode("Faction").InnerText);
        Name = xCharacter.SelectSingleNode("Name").InnerText;
        AI = (StatusAI)int.Parse(xCharacter.SelectSingleNode("AI").InnerText);
        var xCoordinate = xCharacter.SelectSingleNode("Coordinate");
        var posX = int.Parse(xCoordinate.SelectSingleNode("X").InnerText);
        var posY = int.Parse(xCoordinate.SelectSingleNode("Y").InnerText);
        Coordinate = new Vector2(posX,posY);

        this.gameObject.transform.position = Coordinate;

        var xStats = xCharacter.SelectSingleNode("Stats");
        Stats = new Stats();
        Stats.Level = int.Parse(xStats.SelectSingleNode("Level").InnerText);
        Stats.Exp = int.Parse(xStats.SelectSingleNode("Exp").InnerText);
        Stats.TotalHP = int.Parse(xStats.SelectSingleNode("TotalHP").InnerText);
        Stats.HP = int.Parse(xStats.SelectSingleNode("HP").InnerText);
        Stats.MP = int.Parse(xStats.SelectSingleNode("MP").InnerText);
        Stats.Movement = int.Parse(xStats.SelectSingleNode("Movement").InnerText);
        Stats.Str = int.Parse(xStats.SelectSingleNode("Str").InnerText);
        Stats.Dex = int.Parse(xStats.SelectSingleNode("Dex").InnerText);
        Stats.Agi = int.Parse(xStats.SelectSingleNode("Agi").InnerText);
        Stats.Int = int.Parse(xStats.SelectSingleNode("Int").InnerText);
        Stats.Lck = int.Parse(xStats.SelectSingleNode("Lck").InnerText);
    }

    public Character() { 
    
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        Job = GetComponent<Job>();
        Weapon = new Weapon();
        if (Job == null)
            throw new System.ApplicationException("The Character must have an assigned job.");
    }

    void Update() 
    {
        if (animator.GetBool("IsMoving"))
        {
            if (movingTowardIndex < movingTowardCoordinatePath.Count)
            {
                var nextPosition = new Vector3(movingTowardCoordinatePath[movingTowardIndex].x, movingTowardCoordinatePath[movingTowardIndex].y);
                var direction = nextPosition - this.transform.position;
                if (direction.x > 0) { animator.SetBool("MoveRight", true); }
                else { animator.SetBool("MoveRight", false); }
                if (direction.x < 0) { animator.SetBool("MoveLeft",true); }
                else { animator.SetBool("MoveLeft", false); }
                if (direction.y > 0) { animator.SetBool("MoveUp",true); }
                else { animator.SetBool("MoveUp", false); }
                if (direction.y < 0) { animator.SetBool("MoveDown",true); }
                else { animator.SetBool("MoveDown", false); }

                var movement = Vector2.Lerp(this.Coordinate, nextPosition, (float)((double)1 / movingTowardCoordinatePath.Count)) - this.Coordinate;
                this.transform.position += new Vector3(movement.x, movement.y);
                if (this.transform.position == nextPosition)
                {
                    this.Coordinate = new Vector2(this.transform.position.x, this.transform.position.y);
                    movingTowardIndex++;
                    return;
                }
            }
            if (movingTowardIndex >= movingTowardCoordinatePath.Count)
            {
                DoMove();
            }
        }
    }

    /// <summary>
    /// Move the character to the target location
    /// </summary>
    internal void Move(Vector2 to, List<Vector2> Path)
    {
        IsMoving = true;
        lastCoordinate = new Vector2(this.Coordinate.x, this.Coordinate.y);
        animator.SetBool("IsMoving", true);
        movingTowardCoordinate = to;
        Path.Add(to);
        movingTowardCoordinatePath = Path;
        movingTowardIndex = 0;
    }

    internal void DoMove()
    {
        IsMoving = false;
        this.transform.position = movingTowardCoordinate;
        Coordinate = movingTowardCoordinate;   
        movingTowardIndex = 0;
        HasMoved = true;
        
        animator.SetBool("IsMoving", false);
        animator.SetBool("MoveLeft", false);
        animator.SetBool("MoveRight", false);
        animator.SetBool("MoveUp", false);
        animator.SetBool("MoveDown", false);
        CharacterMoved(new CharacterMovedEventArgs { character = this });
    }

    internal void UnMove() 
    {
        this.transform.position = lastCoordinate;
        Coordinate = lastCoordinate;
        HasMoved = false;
    }

    internal void GetHit(int damage)
    {
        this.Stats.HP -= damage;
        StartCoroutine(DoGetHit(damage));
    }

    internal IEnumerator DoGetHit(int damageTaken) {
        CharacterGotHit(new CharacterGotHitEventArgs { character = this, damage = damageTaken });
        if (this.Stats.HP <= 0)
        {
            Destroy(this.gameObject); //breaks animator.
        }
        yield return new WaitForSeconds(1f);
    }

    internal void GainExp(int exp)
    {
        if (this.Stats.Exp + exp > 100)
        {
            this.Stats.Level += 1; //TODO: Level Up.
            this.Stats.Exp = this.Stats.Exp + exp - 100;
        }
        this.Stats.Exp += exp;
    }

    internal virtual void Attack(Vector2 coordinate)
    {
        IsAttacking = true;

        if (this.Coordinate.x - coordinate.x > 0)
        {
            animator.SetBool("AttackUp", true);
            return;
        }
        if (this.Coordinate.x - coordinate.x < 0)
        {
            animator.SetBool("AttackDown", true);
            return;
        }
        if (this.Coordinate.y - coordinate.y > 0)
        {
            animator.SetBool("AttackDown", true);
            return;
        }
        if (this.Coordinate.y - coordinate.y < 0)
        {
            animator.SetBool("AttackUp", true);
            return;
        }
    }

    internal void UseSkill(Skill s) {
        Debug.Log("use skill?");
        animator.SetBool("UseSkill", true);
        IsAttacking = true;
    }

    internal void DoUseSkill() 
    {
        animator.SetBool("UseSkill", false);
        IsAttacking = false;
        HasFinished = true;
        CharacterUsedSkill(new CharacterUsedSkillEventArgs { character = this });
    }

    internal void DoAttack() 
    {
        animator.SetBool("AttackDown", false);
        animator.SetBool("AttackUp", false);

        IsAttacking = false;
        HasFinished = true;
        CharacterAttacked(new CharacterAttackedEventArgs { character = this });
    }

    internal void UseItem()
    {
        throw new NotImplementedException();
    }

    public void RestartTurn()
    {
        HasFinished = false;
        HasMoved = false;
    }

    #region eventos
    /* Hotfix para unity que no me deja usar las clases del padre.*/
    internal void EnviarEvento(CharacterAttackedEventArgs e) {
        CharacterAttacked(e);
    }
    internal void EnviarEvento(CharacterGotHitEventArgs e)
    {
        CharacterGotHit(e);
    }
    internal void EnviarEvento(CharacterMovedEventArgs e)
    {
        CharacterMoved(e);
    }
    internal void EnviarEvento(CharacterUsedSkillEventArgs e)
    {
        CharacterUsedSkill(e);
    }
    #endregion

}
