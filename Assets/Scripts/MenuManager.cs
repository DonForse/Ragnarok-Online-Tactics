using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public Pointer pointer;
    public GameObject Menu;
    public GameObject MenuSkills;
    public GameObject ButtonSkill;
    internal Character CharacterDoingAction;


    public void OpenMenu(Character c)
    {
        CharacterDoingAction = c;
        pointer.Disable();
        Menu.SetActive(true);
        var theButton = Menu.transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(null); //HOTFIX: Bug de Unity.
        EventSystem.current.SetSelectedGameObject(theButton);
    }

    private void ReOpenMenu()
    {
        Menu.SetActive(true);
        var theButton = Menu.transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(null); //HOTFIX: Bug de Unity.
        EventSystem.current.SetSelectedGameObject(theButton);
    }

    public void CloseMenu()
    {
        pointer.Enable();
        CharacterDoingAction = null;
        Menu.SetActive(false);
    }

    public void OpenSkillsMenu()
    {
        Menu.SetActive(false);
        pointer.Disable();

        //MenuSkills.GetComponent<RectTransform>().anchorMax += new Vector2(0, 25 * CharacterDoingAction.Job.Skills.Count);
        //MenuSkills.GetComponent<RectTransform>().anchorMin -= new Vector2(0, 25 * CharacterDoingAction.Job.Skills.Count);


        EventTrigger.Entry cancel = new EventTrigger.Entry();
        cancel.eventID = EventTriggerType.Cancel;
        cancel.callback = new EventTrigger.TriggerEvent();

        foreach (var skill in CharacterDoingAction.Job.Skills)
        {
            var buttonSkill = Instantiate<GameObject>(ButtonSkill);
            buttonSkill.name = ((int)skill.Id).ToString();

            buttonSkill.transform.SetParent(MenuSkills.transform, false);
            var buttonComponent = buttonSkill.gameObject.GetComponent<UnityEngine.UI.Button>();
            var name = buttonSkill.transform.FindChild("Name");
            name.GetComponent<UnityEngine.UI.Text>().text = skill.Name;

            buttonComponent.onClick.AddListener(UsedSkill);

            EventTrigger eventTrigger = buttonComponent.GetComponent<EventTrigger>();

            ////Create a new trigger to hold our callback methods
            cancel.callback = new EventTrigger.TriggerEvent();

            ////Create a new UnityAction, it contains our DropEventMethod delegate to respond to events
            UnityEngine.Events.UnityAction<BaseEventData> call = new UnityEngine.Events.UnityAction<BaseEventData>(OnSkillCancel);

            ////Add our callback to the listeners
            cancel.callback.AddListener(call);

            ////Add the EventTrigger entry to the event trigger component
            eventTrigger.triggers.Add(cancel);
        }
        MenuSkills.SetActive(true);

        var theButton = MenuSkills.transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(null); //HOTFIX: Bug de Unity.
        EventSystem.current.SetSelectedGameObject(theButton);
    }


    private void UsedSkill()
    {
        CloseSkillsMenu(false);
        var id = int.Parse(EventSystem.current.currentSelectedGameObject.name);
        var skill = CharacterDoingAction.Job.Skills.FirstOrDefault(s => (int)s.Id == id);
        if (skill == null) // Error.
        {
            ReOpenMenu();
            return;
        }
        MenuAction(new MenuEventArgs { Action = MenuActionEvent.Skills,SkillSelected = skill });
    }

    public void OnSkillCancel(BaseEventData o)
    {
        CloseSkillsMenu(true);
    }

    public void CloseSkillsMenu(bool returnToMenu) 
    {
        MenuSkills.SetActive(false);
        var cantidadHijos = MenuSkills.transform.childCount;
        for (int i = cantidadHijos - 1; i >= 0; i--)
        {
            Destroy(MenuSkills.transform.GetChild(i).gameObject);
        }
        
        if (returnToMenu) 
            ReOpenMenu();
        else
            pointer.Enable();
    }



    public void OnCancel() 
    {
        MenuAction(new MenuEventArgs { Action = MenuActionEvent.Back });
    }

    public void Attack()
    {
        MenuAction(new MenuEventArgs { Action = MenuActionEvent.Attack });
    }
    public void Wait()
    {
        print("wait");
        MenuAction(new MenuEventArgs { Action = MenuActionEvent.Wait });
    }

    public void Skills() {
        OpenSkillsMenu();
    }

    public void Items() { 
    
    }



    public delegate void OnMenuAction(MenuEventArgs e);
    public static event OnMenuAction MenuAction;

    public class MenuEventArgs : EventArgs
    {
        public MenuActionEvent Action { get; set; }
        public Skill SkillSelected;
    }
}
