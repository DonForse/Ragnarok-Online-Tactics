using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyAIManager: MonoBehaviour
{
    private Character[] Characters;
    private int IndexCharacterTurn = 0;
    private bool doingFactionTurn = false;
    private bool doingTurn = false;

    public IEnumerator StartTurn(Faction f) 
    {
        if (doingFactionTurn)
        {
            Debug.LogError("Como llego?");
            yield return new WaitForSeconds(2.0f);
        }
        Characters = Map.Instance.GetCharactersFromTeam(f).ToArray();
        if (Characters.Length == 0)
        {
            Debug.LogError("No hay enemigos");
            EnemyAIEndTurn();
            yield return new WaitForSeconds(2.0f);
        }

        IndexCharacterTurn = 0;
        doingFactionTurn = true;
        InvokeRepeating("DoTurn", 1.5f, 1);
    }

    private void DoTurn()
    {
        //TODO: Si matas a todos se cuelga.
        if (Characters.Length == IndexCharacterTurn)
        {
            CancelInvoke("DoTurn");
            doingFactionTurn = false;
            EnemyAIEndTurn();
            return;
        }
        if (doingTurn)
            return;

        var c = Characters[IndexCharacterTurn];
        try
        {
            doingTurn = true;

            // Si ya termino, termina.
            if (c.HasFinished)
                return;

            // Si esta moviendo, termina.
            if (c.IsMoving)
                return;

            // Si esta atacando, termina.
            if (c.IsAttacking)
                return;

            var closestEnemyUnit = Map.Instance.GetClosestEnemyUnit(c);

            if (closestEnemyUnit == null)
            { //TODO: End Game.
                Debug.LogError("TERMINO EL JUEGO!");
                c.HasFinished = true;
                CancelInvoke("DoTurn");
                EnemyAIEndTurn();
                return;
            }

            if (c.AI == StatusAI.Defensive)
            {
                Map.Instance.ShowRangeForAttack(c);
                if (Map.Instance.IsInRangeForAttack(c, closestEnemyUnit.Coordinate))
                {
                    Map.Instance.AttackCharacter(c.Coordinate, closestEnemyUnit.Coordinate);
                }
                else {
                    c.HasFinished = true;
                }
                Map.Instance.HideRangeForAttack();
            }

            else if (c.AI == StatusAI.Aggressive)
            {
                //Si no se esta moviendo, y no se movio, hacer el movimiento de moverse.
                if (!c.HasMoved)
                {
                    //Get closest unit.
                    Map.Instance.ShowRange(c);
                    var targetCoordinate = Map.Instance.GetShortestDistanceToTarget(c, closestEnemyUnit.Coordinate);
                    if (Map.Instance.IsInRange(c, targetCoordinate))
                    {
                        Map.Instance.MoveCharacter(c.Coordinate, targetCoordinate);
                    }
                    Map.Instance.HideRange();
                    return;
                }

                if (!c.HasFinished)
                {
                    //Si no se esta moviendo, y ya se movio, ataca.
                    Map.Instance.ShowRangeForAttack(c);
                    if (Map.Instance.IsInRangeForAttack(c, closestEnemyUnit.Coordinate))
                    {
                        Map.Instance.AttackCharacter(c.Coordinate, closestEnemyUnit.Coordinate);
                    }
                    else {
                        c.HasFinished = true;
                    }
                    Map.Instance.HideRangeForAttack();
                }
            }

        }
        finally 
        {
            Debug.Log(IndexCharacterTurn);
            if (c.HasFinished)
                IndexCharacterTurn++;
            doingTurn = false;
        }
    }


    public delegate void OnEnemyAIEndTurn();
    public static event OnEnemyAIEndTurn EnemyAIEndTurn;
}
