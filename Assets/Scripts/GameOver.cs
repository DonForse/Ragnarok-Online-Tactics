using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Invoke("StartAgain", 10f);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.anyKeyDown) {
            StartAgain();
        }
	}
    void StartAgain() {
        Application.LoadLevel("StartMenu");
    }
}
