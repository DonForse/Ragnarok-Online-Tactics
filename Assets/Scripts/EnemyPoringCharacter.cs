using System;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
//using Object = UnityEngine.Object;

public class EnemyPoringCharacter : Character
{
    public EnemyPoringCharacter()
    { 
    
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
                if (direction.y > 0) { animator.SetBool("MoveRight",true); }
                else { animator.SetBool("MoveRight", false); }
                if (direction.y < 0) { animator.SetBool("MoveLeft", true); }
                else { animator.SetBool("MoveLeft", false); }

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
        EnviarEvento(new CharacterMovedEventArgs { character = this });
    }

    internal override void Attack(Vector2 coordinate)
    {
        IsAttacking = true;

        if (this.Coordinate.x - coordinate.x > 0)
        {
            animator.SetBool("AttackLeft", true);
            return;
        }
        if (this.Coordinate.x - coordinate.x < 0)
        {
            animator.SetBool("AttackRight", true);
            return;
        }
        if (this.Coordinate.y - coordinate.y > 0)
        {
            animator.SetBool("AttackRight", true);
            return;
        }
        if (this.Coordinate.y - coordinate.y < 0)
        {
            animator.SetBool("AttackLeft", true);
            return;
        }
    }

    internal void DoAttack() 
    {
        animator.SetBool("AttackRight", false);
        animator.SetBool("AttackLeft", false);

        IsAttacking = false;
        HasFinished = true;
        EnviarEvento(new CharacterAttackedEventArgs { character = this });
    }

    internal void DoAttackForAnimation() {
        this.DoAttack();
    }
}
