using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public int Rows;
    public int Columns;
    public GameObject CharacterGO;
    public GameObject EnemyCharacterGO;
    public GameObject TileForMovement;
    public EnemyAIManager EnemyAIManager;
    public MenuManager MenuManager;
    public UIManager UIManager;

    public bool IsPlayerTurn;
    public Faction PlayerFaction;
    public static Queue<Faction> TeamsOrder;
    private Faction currentTurnPlayer;

    private Character SelectedCharacter = null;
    private Skill SelectedSkill = null;

    private Action Action;

    private static Map Map;

    public delegate void OnGameEnded(bool victory);
    public static event OnGameEnded GameEnded;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsPlayerTurn = true;
        Map.Create(Rows, Columns, CharacterGO, EnemyCharacterGO, TileForMovement, UIManager);
        Map = Map.Instance;
    }

    void Start()
    {
        Pointer.PointerAction += OnActionPointer;
        Pointer.PointerCancel += OnCancelPointer;
        Character.CharacterMoved += OnCharacterMoved;
        Character.CharacterAttacked += OnCharacterAttacked;
        Character.CharacterUsedSkill += OnCharacterUsedSkill;
        MenuManager.MenuAction += OnMenuAction;
        EnemyAIManager.EnemyAIEndTurn += OnEnemyAIEndTurn;
        GameEnded += OnFinishedLevel;

        Action = Action.None;
        CheckEndTurn();
    }

    private void OnFinishedLevel(bool victory) 
    {
        if (!victory)
            Application.LoadLevel("GameOver");
        else
            Debug.LogError("Victory");
    }

    /// <summary>
    /// Does the "Action" button action send from the pointer.
    /// </summary>
    private void OnActionPointer(Pointer.PointerEventArgs e)
    {
        if (!IsPlayerTurn)
            return;

        var c = Map.GetCharacterFromTile(e.Coordinate);

        if (c == null) 
        {
            switch (Action)
            {
                case Action.Move:
                    if (MoveCharacter(e.Coordinate))
                    {
                        print("eligio una celda vacia para moverse.");
                        Pointer.EnableActions = false;
                    }
                    else
                    {
                        print("eligio una celda fuera de rango de movimiento.");
                        return;
                    }
                    break;

                case Action.Attack: //isAttacking) 
                    if (AttackCharacter(c))
                        Pointer.EnableActions = false;
                    else
                        print("clickeo una celda vacia al estar atacando");
                    break;

                case Action.Skill:
                    if (UseSkillCharacter(c))
                        Pointer.EnableActions = false;
                    else
                        print("clickeo una celda vacia al estar usando un skill");
                    break;
                default:
                    print("clickeo una celda vacia, sin haber seleccionado un personaje antes");
                    break;
            }
        }

        if (c != null) //TODO: despues de checkear todas las instancias cambiar el if a else, xq es mas performante.
        {
            if (c.Team != PlayerFaction)
            {
                if (Action != Action.Attack && Action != Action.Skill)//!isAttacking && !isUsingSkill)
                {
                    print("clickeo un personaje enemigo por primera vez, no para atacar.");
                }
                else if (Action == Action.Attack )//isAttacking) //depurar si no hay mas condiciones a solo else.
                {
                    print("clickeo a un personaje enemigo, cuando estaba atacando");
                    if (AttackCharacter(c)) {
                        Pointer.EnableActions = false;
                    }
                }
                else if (Action == Action.Skill) 
                { 
                    print ("clickeo a un personaje enemigo, cuando estaba usando un skill");
                    if (UseSkillCharacter(c))
                    {
                        Pointer.EnableActions = false;
                    }
                }
            }
            else 
            {
                if (Action == Action.None)//!isAttacking && !isMoving) //Eligio un personaje para actuar por primera vez.
                {
                    print("clickeo a un personaje propio, para inciar su accion");
                    if (c.HasMoved || c.HasFinished)
                    { //Character already moved (should never enter here) (for example clicking an already moved character) or finished his turn.
                        Debug.Log(c.HasMoved);
                        Debug.Log(c.HasFinished);
                        return; //OpenOptionsMenu.
                    }
                    Map.ShowRange(c);
                    SelectedCharacter = c;
                    Action = Action.Move;//isMoving = true;
                }
                else if (Action == Action.Move && c != SelectedCharacter) // isMoving 
                {
                    print("clickeo a un personaje propio, cuando se estaba moviendo"); //Ultimo cambio deberia ser imposible q pase.
                }
                else if (Action == Action.Move && c == SelectedCharacter) //isMoving
                {
                    if (MoveCharacter(e.Coordinate))
                    {
                        print("eligio al mismo personaje, para no moverse y abrir el menu.");
                        Pointer.EnableActions = false;
                    }
                    else
                    {
                        //print("eligio una celda fuera de rango de movimiento.");
                        return;
                    }
                }
                else if (Action == Action.Attack)//isAttacking)
                {
                    print("clickeo a un personaje aliado, cuando estaba atacando");
                    if (AttackCharacter(c))
                    {
                        Pointer.EnableActions = false;
                    }
                }
                else if (Action == Action.Skill)//isAttacking)
                {
                    print("clickeo a un personaje aliado, cuando estaba usando una skill");
                    if (UseSkillCharacter(c))
                    {
                        Pointer.EnableActions = false;
                    }
                }
            }
        }
        
        //TODO: Ver el caso se clickear un personaje aliado luego de otro
    }

    /// <summary>
    /// Does the "Cancel" button action.
    /// </summary>
    /// <remarks>Next time use a Command Pattern.</remarks>
    private void OnCancelPointer()
    {
        if (!IsPlayerTurn)
            return;

        if (SelectedCharacter == null)
            return;

        if (SelectedCharacter.HasFinished)
            return;

        if (Map.IsShowingRange())
        {
            Map.HideRange();
            //isMoving = false;
            Action = Action.None;
            SelectedCharacter = null;
            return;
        }

        Debug.Log(Action);
        if (Action == Action.Attack || Action == Action.Skill)//isAttacking) 
        {
            Action = Action.None;//isAttacking = false;
            Map.HideRangeForAttack();
            MenuManager.OpenMenu(SelectedCharacter);
            print("Esta atacando y se quizo cancelar la accion");
            return;
        }

        if (SelectedCharacter.HasMoved)
        {
            SelectedCharacter.UnMove();
            SelectedCharacter = null;
            return;
        }
    }

    private void OnMenuAction(MenuManager.MenuEventArgs e)
    {
        switch (e.Action) 
        {
            case MenuActionEvent.Attack:
                ShowAttackRange();
                break;
            case MenuActionEvent.Back:
                MenuManager.CloseMenu();
                OnCancelPointer();
                break;
            case MenuActionEvent.Items:
                break;
            case MenuActionEvent.Skills:
                SelectedSkill = e.SkillSelected;
                ShowSkillRange();
                break;
            case MenuActionEvent.Wait:
                WaitCharacter();
                break;
        }
        //CheckEndTurn(); //Aca no, en Items y Wait  si, en los otros, en el skill que se use.
    }

    private void OnCharacterMoved(Character.CharacterMovedEventArgs e)
    {
        Pointer.EnableActions = true;
        if (e.character.Team == PlayerFaction) // Is player controlled
        {
            Action = Action.None;//isMoving = false;
            MenuManager.OpenMenu(SelectedCharacter);
        }
    }

    private void OnCharacterAttacked(Character.CharacterAttackedEventArgs e)
    {
        Pointer.EnableActions = true;
        if (e.character.Team == PlayerFaction) // Is player controlled
        {
            Action = Action.None;//this.isAttacking = false;
            CheckEndTurn();
        }
    }

    private void OnCharacterUsedSkill(Character.CharacterUsedSkillEventArgs e)
    {
        Pointer.EnableActions = true;
        Debug.Log("skill");
        if (e.character.Team == PlayerFaction) // Is player controlled
        {
            Debug.Log("skill");
            Action = Action.None;//this.isAttacking = false;
            CheckEndTurn();
        }
    }


    private void WaitCharacter() {
        if (SelectedCharacter != null)
            SelectedCharacter.HasFinished = true;
        else 
        {
            print("fuck");
        }
        MenuManager.CloseMenu();
        if (Map.Instance.GetTile(SelectedCharacter.Coordinate).Name == "Gate") {
            GameEnded(true);
            Debug.LogError("Win!");
        }
        CheckEndTurn();
    }

    // no deberia tener target, ataca a un lugar, si hay o no target cambia el resultado.
    private bool ShowAttackRange()
    {
        if (Map.ShowRangeForAttack(SelectedCharacter))
            Action = Action.Attack;
        MenuManager.CloseMenu();
        return true;
    }

    private bool ShowSkillRange()
    {
        if (Map.ShowRangeForSkill(SelectedCharacter, SelectedSkill))
            Action = Action.Skill;
        MenuManager.CloseMenu();
        return true;
    }

    private bool AttackCharacter(Character c)
    {
        if (c == null)
            return false; //Do Animation attacking nothing.

        if (!Map.IsInRangeForAttack(SelectedCharacter,c.Coordinate))
            return false;
        //Chequear rango.

        if (!Map.AttackCharacter(SelectedCharacter.Coordinate, c.Coordinate))
            return false;
        Map.HideRangeForAttack();

        return true;
    }

    private bool UseSkillCharacter(Character c)
    {
        if (c == null)
            return false; //Do Animation attacking nothing.

        if (!Map.IsInRangeForSkill(SelectedCharacter, SelectedSkill, c.Coordinate))
            return false;
        //Chequear rango.

        if (!Map.UseSkill(SelectedCharacter.Coordinate, SelectedSkill, c.Coordinate))
            return false;
        Map.HideRangeForSkill();

        return true;
    }

    private bool MoveCharacter(Vector2 pointerCoordinate)
    {
        if (SelectedCharacter.HasMoved)
            return false;

        if (!Map.IsInRange(SelectedCharacter, pointerCoordinate))
            return false;

        if (!Map.MoveCharacter(SelectedCharacter.Coordinate, pointerCoordinate))
            return false;

        Map.HideRange();
        return true;
    }

    private void CheckEndTurn()
    {
        print(currentTurnPlayer);
        if (Map.CheckAllFinished(currentTurnPlayer))
        {
            currentTurnPlayer = TeamsOrder.Dequeue();
            TeamsOrder.Enqueue(currentTurnPlayer);
            IsPlayerTurn = currentTurnPlayer == PlayerFaction;

            UIManager.ShowPlayerTurn(currentTurnPlayer, IsPlayerTurn);

            //lastCharacterMoved = null;
            //lastCharacterSelected = null;
            SelectedCharacter = null;
            Map.ResetTurn(currentTurnPlayer);

            if (IsPlayerTurn == false)
                EnemyTurn();
        }
    }

    private void EnemyTurn()
    {
        //DoThings.
        
        StartCoroutine (EnemyAIManager.StartTurn(currentTurnPlayer));
    }

    private void OnEnemyAIEndTurn() 
    {       
        if (Map.GetCharactersFromTeam(PlayerFaction).Count <= 0)
            GameEnded(false);
        CheckEndTurn();
    }
    //LoseCondition :All Allies killed.
}
