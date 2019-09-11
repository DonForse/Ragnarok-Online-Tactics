using UnityEngine;
using System.Collections;

public class TextoDaño: MonoBehaviour {

    public void Create(string text, Vector2 coordinate, Color32 c) 
    {
        this.transform.position = new Vector3(coordinate.x, coordinate.y + 0.5f, -1);
        var texto = this.GetComponent<TextMesh>();
        texto.text = text;
        texto.color = c;
        var animator = this.GetComponent<Animator>();
        this.gameObject.SetActive(true);
        animator.SetTrigger("TextAnimation");
    }

    public void Destroy() 
    {
        Destroy(gameObject);
    }
}
