using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Xml;

public class Map
{
    #region Singleton
    private static Map instance;

    /// <summary>
    /// Instancia unica
    /// </summary>
    public static Map Instance
    {
        get
        {
            return instance;
        }
    }

    private Map() { }
    private Map(int rows, int columns, GameObject character, GameObject enemyCharacter, GameObject movementTile, UIManager ui)
    {
        //HOTFIX: Use Static class with a monobehavior.
        tilesForMovement = new List<GameObject>();
        tilesForAttack = new List<GameObject>();
        lastDijkstraSearch = new List<DijkstraTile>();
        lastDijkstraSearchForAttack = new List<DijkstraTile>();

        EnemyCharacter = enemyCharacter;
        ps = character;
        MovementTile = movementTile;

        UIManager = ui;
        Rows = rows;
        Columns = columns;
        
        Tiles = GetMap();
        Personajes = GetCharacters();

        Pointer.PointerMoved += ShowPath;
        GameManager.TeamsOrder = new Queue<Faction>(GetFactions());
    }

    public static void Create(int rows, int columns, GameObject character,GameObject enemyCharacter, GameObject movementTile, UIManager ui)
    {
        if (instance != null)
        {
            throw new Exception("Object already created");
        }
        instance = new Map(rows, columns, character, enemyCharacter, movementTile, ui);
    } 
    #endregion

    //Ver ajustar medidas de los tiles al mapa / viceversa.

    internal IList<Tile> Tiles;
    internal List<Character> Personajes;

    internal int Rows;
    internal int Columns;

    private GameObject ps;
    private GameObject EnemyCharacter;
    private GameObject MovementTile;
    private UIManager UIManager;

    private IList<GameObject> tilesForMovement;
    private IList<GameObject> tilesForAttack;
    private IList<DijkstraTile> lastDijkstraSearch;
    private IList<DijkstraTile> lastDijkstraSearchForAttack;

    public List<Tile> GetMap() 
    {        
        var mapParts = Resources.LoadAll<Sprite>("Map");
        
        foreach (var part in mapParts) {
            var go = new GameObject();
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = part;
            Vector2 position = new Vector2(part.uv[0].x * 15, part.uv[0].y *10 -1);
            sr.transform.position = position;
            
            var t = GameObject.Instantiate<GameObject>(go); //cant put in parent.. preguntar.
        }
        
        var mapa = new List<Tile>();

        int row = 0;
        using (var reader = new StreamReader(@"Assets\Data\map.txt"))
        {
            var line = reader.ReadLine();
            while (line != null)
            {
                var tiles = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                int col = 0;
                foreach (var tile in tiles)
                {
                    var t = new Tile();
                    switch (tile) {
                        case "m":
                            t.Name = "Mountain";
                            t.Type = TileType.Mountain;
                            t.MovementCost = 2;
                            t.Defense = 2;
                            t.AvoidValue = 30;
                            break;
                        case "M":
                            t.Name = "Peak";
                            t.Type = TileType.Peak;
                            t.MovementCost = 3;
                            t.Defense = 2;
                            t.AvoidValue = 40;
                            break;
                        case "T":
                            t.Name = "Forest";
                            t.Type = TileType.Forest;
                            t.MovementCost = 2;
                            t.Defense = 1;
                            t.AvoidValue = 20;
                            break;
                        case "C":
                            t.Name = "Cliff";
                            t.Type = TileType.Cliff;
                            t.MovementCost = 1000;
                            t.Defense = 0;
                            t.AvoidValue = 0;
                            break;
                        case "Q":
                            t.Name = "Castle";
                            t.Type = TileType.Castle;
                            t.MovementCost = 1000;
                            t.Defense = 0;
                            t.AvoidValue = 0;
                            break;
                        case "c":
                            t.Name = "Gate";
                            t.Type = TileType.Gate;
                            t.AvoidValue = 20;
                            t.Defense = 2;
                            t.MovementCost = 1;
                            break;
                        case "P":
                            t.Name = "Plain";
                            t.Type = TileType.Plain;
                            t.AvoidValue = 0;
                            t.Defense = 0;
                            t.MovementCost = 1;
                            break;
                        case "H":
                            t.Name = "Home";
                            t.Type = TileType.Home;
                            t.AvoidValue = 0;
                            t.Defense = 0;
                            t.MovementCost = 2;
                            break;
                        default:
                            Debug.Log(tile);
                            break;
                    }
                    t.Coordinate = new Vector2(col, row);
                    mapa.Add(t);
                    col++;
                }
                row++;
                line = reader.ReadLine();
            }
        }
        return mapa;
    }

    public List<Character> GetCharacters()
    {
        List<Character> characters = new List<Character>();
        XmlDocument doc = new XmlDocument();
        doc.Load(@"Assets\Data\Characters.xml");
        var xCharactersRoot = doc.SelectSingleNode("Characters");
        var xCharacters = xCharactersRoot.SelectNodes("Character");
        foreach (XmlNode xCharacter in xCharacters) 
        {
            GameObject c;
            if ((StatusAI)int.Parse(xCharacter.SelectSingleNode("AI").InnerText) == StatusAI.None)
                c = GameObject.Instantiate(ps);
            else
                c = GameObject.Instantiate(EnemyCharacter);
            var character = c.GetComponent<Character>();
            character.ParseCharacter(xCharacter);
            characters.Add(character);
        }
        return characters;
    }

    /// <summary>
    /// Returns the character in the specified position
    /// </summary>
    /// <param name="coord">x/y map coordinate of the tile</param>
    /// <returns>Character or null if there wasn't a character in the position.</returns>
    public Character GetCharacterFromTile(Vector2 coord)
    {
        return Personajes.FirstOrDefault(p => p.Coordinate == coord);
    }

    /// <summary>
    /// Moves a character from a location to another.
    /// </summary>
    /// <param name="from">position of the character</param>
    /// <param name="to">destination position</param>
    /// <returns>true/false if the character could move.</returns>
    public bool MoveCharacter(Vector2 from, Vector2 to)
    {        
        var personaje = Personajes.FirstOrDefault(p => p.Coordinate == from );

        if (personaje == null)
            return false;

        var tileFinal = lastDijkstraSearch.FirstOrDefault(d => d.t.Coordinate == to);

        var path = tileFinal.Path.Select(p => p.Coordinate).ToList();
        personaje.Move(to,path);


        return true;
    }

    public Tile GetTile(Vector2 coordinate) 
    {
        return Tiles.FirstOrDefault(t => t.Coordinate == coordinate);
    }

    public IEnumerable<Faction> GetFactions()
    {
        return Personajes.Select(p=>p.Team).Distinct();
    }

    #region Movement Range

    public bool IsInRange(Character c, Vector2 coord)
    {
        return lastDijkstraSearch.Any(d => d.t.Coordinate == coord && d.Distance <= c.Stats.Movement);
    }

    public bool HideRange()
    {
        foreach (var obj in tilesForMovement)
        {
            GameObject.Destroy(obj);
        }
        tilesForMovement = new List<GameObject>();
        lastDijkstraSearch = new List<DijkstraTile>();
        return true;
    }

    public bool ShowRange(Character c)
    {
        var startTile = Tiles.FirstOrDefault(t => t.Coordinate == c.Coordinate);
        var maxRangeOfTiles = Tiles.Where(t => ((t.Coordinate.x <= c.Coordinate.x + c.Stats.Movement) && (t.Coordinate.x >= c.Coordinate.x - c.Stats.Movement))
            && ((t.Coordinate.y <= c.Coordinate.y + c.Stats.Movement) && (t.Coordinate.y >= c.Coordinate.y - c.Stats.Movement))).ToList();

        var blockedTiles = Tiles.Where(t=>Personajes.Any(p=>p.Coordinate == t.Coordinate && p != c)).ToList();

        var dijks = Dijsktra.GetDistances(maxRangeOfTiles, startTile, blockedTiles);

        foreach (var dijk in dijks)
        {
            if (dijk.Distance > c.Stats.Movement)
                continue;

            //if (Personajes.Where(p => p.Team != c.Team).Any(p => p.Coordinate == dijk.t.Coordinate))
            if (Personajes.Where(p => p != c).Any(p => p.Coordinate == dijk.t.Coordinate))
                continue;

            GameObject newObject = GameObject.Instantiate(MovementTile) as GameObject;
            newObject.transform.position = dijk.t.Coordinate;
            tilesForMovement.Add(newObject);
            lastDijkstraSearch.Add(dijk);
        }
        
        return true;
    }


    public Vector2 GetShortestDistanceToTarget(Character c, Vector2 targetCoordinate) 
    {
        var minDistance = lastDijkstraSearch.Min(l => Vector2.Distance(l.t.Coordinate, targetCoordinate));
        return lastDijkstraSearch.FirstOrDefault(l => Vector2.Distance(l.t.Coordinate, targetCoordinate) == minDistance).t.Coordinate;
    }
    #endregion

    #region Attack Range

    public bool IsInRangeForAttack(Character c, Vector2 coord)
    {
        return lastDijkstraSearchForAttack.Any(d => d.t.Coordinate == coord && d.Distance <= c.Weapon.Range);
    }

    public bool HideRangeForAttack()
    {
        foreach (var obj in tilesForAttack)
        {
            GameObject.Destroy(obj);
        }
        tilesForAttack = new List<GameObject>();
        lastDijkstraSearchForAttack = new List<DijkstraTile>();
        return true;
    }

    public bool ShowRangeForAttack(Character c)
    {
        var startTile = Tiles.FirstOrDefault(t => t.Coordinate == c.Coordinate);
        var tilesInRange = Tiles.Where(t => ((t.Coordinate.x <= c.Coordinate.x + c.Weapon.Range) && (t.Coordinate.x >= c.Coordinate.x - c.Weapon.Range))
            && ((t.Coordinate.y <= c.Coordinate.y + c.Weapon.Range) && (t.Coordinate.y >= c.Coordinate.y - c.Weapon.Range))).ToList();

        var dijks = Dijsktra.GetDistancesWithoutCost(tilesInRange, startTile);
        foreach (var dijk in dijks)
        {
            if (dijk.Distance > c.Weapon.Range)
                continue;
            if (dijk.t.Coordinate == c.Coordinate)
                continue;
            //if (!Personajes.Where(p => p.Team != c.Team).Any(p => p.Coordinate == tile.Coordinate))
            //        continue;
            GameObject newObject = GameObject.Instantiate(MovementTile) as GameObject;
            newObject.transform.position = dijk.t.Coordinate;
            newObject.GetComponent<SpriteRenderer>().color = Color.red;
            tilesForAttack.Add(newObject);
            lastDijkstraSearchForAttack.Add(dijk);
        }
        return true;
    }

    #endregion

    #region Skills Range
    public bool ShowRangeForSkill(Character c, Skill s)
    {
        var startTile = Tiles.FirstOrDefault(t => t.Coordinate == c.Coordinate);
        var tilesInRange = Tiles.Where(t => ((t.Coordinate.x <= c.Coordinate.x + s.Range) && (t.Coordinate.x >= c.Coordinate.x - s.Range))
            && ((t.Coordinate.y <= c.Coordinate.y + s.Range) && (t.Coordinate.y >= c.Coordinate.y - s.Range))).ToList();

        var dijks = Dijsktra.GetDistancesWithoutCost(tilesInRange, startTile);
        foreach (var dijk in dijks)
        {
            if (dijk.Distance > s.Range)
                continue;
            //if (!Personajes.Where(p => p.Team != c.Team).Any(p => p.Coordinate == tile.Coordinate))
            //        continue;
            GameObject newObject = GameObject.Instantiate(MovementTile) as GameObject;
            newObject.transform.position = dijk.t.Coordinate;
            newObject.GetComponent<SpriteRenderer>().color = s.Type == SkillType.Heal ? Color.green : Color.red;
            tilesForAttack.Add(newObject);
            lastDijkstraSearchForAttack.Add(dijk);
        }
        return true;
    }

    public bool IsInRangeForSkill(Character c,Skill s, Vector2 coord)
    {
        return lastDijkstraSearchForAttack.Any(d => d.t.Coordinate == coord && d.Distance <= s.Range);
    }

    public bool HideRangeForSkill()
    {
        foreach (var obj in tilesForAttack)
        {
            GameObject.Destroy(obj);
        }
        tilesForAttack = new List<GameObject>();
        lastDijkstraSearchForAttack = new List<DijkstraTile>();
        return true;
    }

    #endregion

    public bool AttackCharacter(Vector2 from, Vector2 to) 
    {
        var attacker = GetCharacterFromTile(from);
        var defender = GetCharacterFromTile(to); //TODO: Change to get all defenders in range (pass range of attack).

        var tile = GetTile(to); //TODO: Idem defender.
        var damage = BattleHelper.CalculateDamage(attacker, defender, tile); // TODO: foreach defender.
        attacker.Attack(to);
        defender.GetHit(damage); //TODO: do "on hit" del defender, pasarle daño, que el mismo defender chequee si muere o no.
        if (defender.Stats.HP <= 0)
        {
            attacker.GainExp(30);
            UIManager.CreateExpText(attacker, 30, 2f);
            Personajes.Remove(defender);
        }
        else
        {
            attacker.GainExp(10);
            UIManager.CreateExpText(attacker, 10, 2f);
        }
        return true;
    }

    public bool UseSkill(Vector2 from, Skill s ,Vector2 to)
    {
        var attacker = GetCharacterFromTile(from);
        var defender = GetCharacterFromTile(to); //TODO: Change to get all defenders in range (pass range of attack).

        var tile = GetTile(to); //TODO: Idem defender.
        var damage = s.Execute(attacker, to); // TODO: foreach defender.
        attacker.UseSkill(s);
        defender.GetHit(damage); //TODO: do "on hit" del defender, pasarle daño, que el mismo defender chequee si muere o no.
        if (defender.Stats.HP <= 0)
        {
            Personajes.Remove(defender);
        }
        return true;
    }

    public bool IsShowingRange()
    {
        return (tilesForMovement.Count > 0);
    }

    public void ShowPath(Pointer.PointerEventArgs e) 
    {
        if (!Map.Instance.IsShowingRange())
            return;

        foreach (var tile in tilesForMovement) {
            tile.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        var tileFinal = lastDijkstraSearch.FirstOrDefault(d => d.t.Coordinate == e.Coordinate);
        if (tileFinal == null)
            return;

        foreach (var tilePath in tileFinal.Path) 
        {
            var tile = tilesForMovement.FirstOrDefault(t=>t.transform.position == new Vector3 (tilePath.Coordinate.x, tilePath.Coordinate.y));
            if (tile != null)
                tile.GetComponent<SpriteRenderer>().color = Color.green;
        }
        var tileDestino = tilesForMovement.FirstOrDefault(t => t.transform.position == new Vector3(e.Coordinate.x, e.Coordinate.y));
        if (tileDestino != null)
            tileDestino.GetComponent<SpriteRenderer>().color = Color.green;
        return;
    }

    public IList<Character> GetCharactersFromTeam(Faction t) {
        return Personajes.Where(p => p.Team == t).ToList();
    }

    /// <summary>
    /// Return the closest enemy unit to the character.
    /// </summary>
    public Character GetClosestEnemyUnit(Character c) {
        var enemyCharacters = Personajes.Where(ch => ch.Team != c.Team).ToList();
        if (enemyCharacters.Count == 0)
            return null;
        var minDistance = enemyCharacters.Min(e=>Vector2.Distance(c.Coordinate, e.Coordinate ));
        return enemyCharacters.FirstOrDefault(e => Vector2.Distance(c.Coordinate, e.Coordinate) == minDistance);
    }

    #region End of Turn

    public bool CheckAllFinished(Faction f)
    {
        return Personajes.Where(p => p.Team == f).All(p => p.HasFinished);
    }

    public void ResetTurn(Faction f) 
    {
        Debug.Log(f);
        var personajes = Personajes.Where(p => p.Team == f);
        foreach (var p in personajes)
        {
            p.HasMoved = false;
            p.HasFinished = false;
        }

    }

    #endregion
}
