using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour {

    void Update() {
        if (Input.anyKeyDown) {
            Return();
        }
    }

    public void Return()
    {
        Application.LoadLevel("StartMenu");
    }
}
