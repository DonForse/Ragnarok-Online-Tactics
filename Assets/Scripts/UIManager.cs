using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {
    
    public TextoDaño TextoDañoGameObject;


    public GameObject CharacterWindow;
    public Sprite CharacterPortraitSprite; //Next iteration make it more dynamic...
    public Sprite EnemyPortraitSprite;

    private static UnityEngine.UI.Text HpText;
    private static UnityEngine.UI.Text ExpText;
    private static UnityEngine.UI.Text NameText;
    private static UnityEngine.UI.Image Portrait;
    private static Sprite CharacterPortrait;
    private static Sprite EnemyPortrait;


    public GameObject StatusWindow;
    private static UnityEngine.UI.Text AvoidText;
    private static UnityEngine.UI.Text DefenseText;
    private static UnityEngine.UI.Text TitleText;
    private static UnityEngine.UI.Image TitleBackground;

    public GameObject TurnMessage;
    private static UnityEngine.UI.Image TurnMessageBackground;
    private static UnityEngine.UI.Text TurnMessageText;

    void Awake()
    {
        var panel = StatusWindow.transform.FindChild("Panel");

        AvoidText = panel.FindChild("Avoid").gameObject.GetComponent<UnityEngine.UI.Text>();
        DefenseText = panel.FindChild("Defense").gameObject.GetComponent<UnityEngine.UI.Text>();
        TitleText = StatusWindow.transform.FindChild("Title").FindChild("Text").gameObject.GetComponent<UnityEngine.UI.Text>();
        TitleBackground = StatusWindow.transform.FindChild("Title").gameObject.GetComponent<UnityEngine.UI.Image>();

        var panelCharacter = CharacterWindow.transform.FindChild("Panel");
        HpText = panelCharacter.FindChild("HP").gameObject.GetComponent<UnityEngine.UI.Text>();
        ExpText = panelCharacter.FindChild("Exp").gameObject.GetComponent<UnityEngine.UI.Text>();
        NameText = panelCharacter.FindChild("Name").gameObject.GetComponent<UnityEngine.UI.Text>();
        Portrait = CharacterWindow.transform.FindChild("CharacterImage").FindChild("Image").GetComponent<UnityEngine.UI.Image>();
        CharacterPortrait = CharacterPortraitSprite;
        EnemyPortrait = EnemyPortraitSprite;


        Pointer.PointerMoved += UpdateTileStatus;
        Character.CharacterGotHit += UpdateCharacterStatus;
        Character.CharacterMoved += UpdateCharacterStatus;
        EnemyAIManager.EnemyAIEndTurn += UpdateCharacterStatus;
    }

	// Use this for initialization
	void Start () 
    {
        TurnMessageText = TurnMessage.transform.FindChild("Text").gameObject.GetComponent<UnityEngine.UI.Text>();
        TurnMessageBackground = TurnMessage.GetComponent<UnityEngine.UI.Image>();
        Character.CharacterGotHit += CreateDamageText;
	}
	
    public void CreateDamageText(Character.CharacterGotHitEventArgs e)
    {
        Color32 color = e.damage >= 0 ? new Color32(155,0,0,255) : new Color32(0, 91, 0, 255); //green.
        var damage = Mathf.Abs(e.damage);
        StartCoroutine(CreateText(e.character.Coordinate, damage, color));
    }

    public void CreateExpText(Character c, int exp, float delay = 1f)
    {
        Color32 color = new Color32(180, 125, 32, 255);
        StartCoroutine(CreateText(c.Coordinate, exp, color, delay));
    }

    private IEnumerator CreateText(Vector2 coordinate, int number, Color32 color, float delay = 0f)
    {
        var obj = Instantiate<TextoDaño>(TextoDañoGameObject);
        obj.Create(number.ToString(), coordinate, color);
        yield return new WaitForSeconds(delay);
    }

    private static void UpdateTileStatus(Pointer.PointerEventArgs e)
    {
        var t = Map.Instance.GetTile(e.Coordinate);
        AvoidText.text = t.AvoidValue.ToString();
        DefenseText.text = t.Defense.ToString();

        if (t.Type == TileType.Cliff || t.Type == TileType.Castle)
        {
            AvoidText.text = "-";
            DefenseText.text = "-";
        }
        TitleText.text = t.Name;
        switch (t.Type) { 
            case TileType.Castle:
            case TileType.Cliff:
                TitleBackground.color = new Color32(200, 200, 200, 190);
                break;
            case TileType.Forest:
                TitleBackground.color = new Color32(0, 135, 15, 190);
                break;
            case TileType.Gate:
            case TileType.Home:
                TitleBackground.color = new Color32(255, 185, 15, 190);
                break;
            case TileType.Mountain:
            case TileType.Peak:
                TitleBackground.color = new Color32(245, 165, 96, 190);
                break;
            case TileType.Plain:
                TitleBackground.color = new Color32(160,230,120,190);
                break;
            default:
                break;
        }
        UpdateCharacterStatus(t.Coordinate);
    }

    private static void UpdateCharacterStatus(Character.CharacterGotHitEventArgs e) {
        UpdateCharacterStatus(e.character.Coordinate);
    }

    private static void UpdateCharacterStatus(Character.CharacterMovedEventArgs e)
    {
        UpdateCharacterStatus(e.character.Coordinate);
    }

    private static void UpdateCharacterStatus()
    {
        UpdateCharacterStatus(new Vector2(-1, -1)); //Hide
    }


    private static void UpdateCharacterStatus(Vector2 coordinate) {
        var c = Map.Instance.GetCharacterFromTile(coordinate);
        if (c != null)
        {
            HpText.text = string.Format("{0}/{1}", c.Stats.HP, c.Stats.TotalHP);
            ExpText.text = string.Format("{0}/100", c.Stats.Exp);
            NameText.text = c.Name;
            HpText.transform.parent.parent.gameObject.SetActive(true);
            if (c.Team == Faction.Cultist)
            {
                Portrait.sprite = CharacterPortrait;
            }
            else {
                Portrait.sprite = EnemyPortrait;
            }
        }
        else
        {
            HpText.transform.parent.parent.gameObject.SetActive(false);
        }
    }

    public void ShowPlayerTurn(Faction f, bool isPlayerTurn) 
    {
        Pointer.EnableActions = false;
        
        TurnMessageBackground.color = isPlayerTurn ? Color.blue : Color.red;
        TurnMessageText.text = isPlayerTurn ? "Ally  Turn" : "Enemy  Turn";// f.ToString();
        TurnMessageBackground.gameObject.SetActive(true);
        Invoke("HidePlayerTurn", 1f);
    }
    private void HidePlayerTurn() {
        Pointer.EnableActions = true;
        TurnMessageBackground.gameObject.SetActive(false);
    }
}
