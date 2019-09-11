using UnityEngine;
using System.Collections;

public class PrincipalMenu : MonoBehaviour {

    public void StartGame() 
    {
        Application.LoadLevel("Demo");
    }
    public void ShowCredits() {
        Application.LoadLevel("Credits");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
