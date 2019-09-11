using UnityEngine;
using System.Collections;
using System;

public class Pointer : MonoBehaviour {

    public class PointerEventArgs : EventArgs
    {
        public Vector2 Coordinate { get; set; }
    }

    public delegate void OnPointerMoved(PointerEventArgs e);
    public static event OnPointerMoved PointerMoved;

    public delegate void OnPointerCancel();
    public static event OnPointerCancel PointerCancel;

    public delegate void OnPointerAction(PointerEventArgs e);
    public static event OnPointerAction PointerAction;

    /// <summary>
    /// Size of the X Axis of the tile.
    /// </summary>
    public float SizeX;
    /// <summary>
    /// Size of the Y Axis of the tile.
    /// </summary>
    public float SizeY;

    public static bool EnableActions;

    internal Vector2 Coordinate;

    private float lastStep = 0f;

    private const float TimeBetweenSteps = 0.1f;

    void Start()
    {
        Coordinate = new Vector2(this.transform.position.x / SizeX, this.transform.position.y / SizeY);
        EnableActions = true;
        PointerMoved(new PointerEventArgs { Coordinate = this.Coordinate });
    }

	// Update is called once per frame
	void Update () {
        if (EnableActions)
        {
            if (Input.GetButtonDown("Action"))
            {
                PointerAction(new PointerEventArgs { Coordinate = this.Coordinate });
                return;
            }
            if (Input.GetButtonDown("Cancel"))
            {
                PointerCancel();
                return;
            }
        }
        PointerMovement();
	}

    private void PointerMovement()
    {
        if (ClassicMovement())
        {
            lastStep = 0;
            PointerMoved(new PointerEventArgs { Coordinate = this.Coordinate });
        }

        lastStep += Time.deltaTime;
        if (lastStep < TimeBetweenSteps)
            return;

        lastStep = 0f;
        
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        if (v == 0 && h == 0)
            return;

        if (!IsMovingInMap(v, h))
            return;

        this.transform.Translate(Vector2.up * v * SizeY);
        Coordinate.y += v;

        this.transform.Translate(Vector2.right * h * SizeX);
        Coordinate.x += h;

        PointerMoved(new PointerEventArgs { Coordinate = this.Coordinate });
    }

    private bool IsMovingInMap(float movementY, float movementX) {

        if (Coordinate.y + movementY > Map.Instance.Rows || Coordinate.y + movementY < 0)
            return false;
        if (Coordinate.x + movementX > Map.Instance.Columns || Coordinate.x + movementX < 0)
            return false;
        return true;
    }

    private bool ClassicMovement()
    {
        if (Input.GetButtonDown("Left"))
        {
            if (Coordinate.x - 1 < 0)
                return false;

            this.transform.Translate(Vector2.left * SizeX);
            Coordinate.x -= 1;
            return true;
        }
        if (Input.GetButtonDown("Right"))
        {
            if (Coordinate.x + 1 > Map.Instance.Columns)
                return false;

            this.transform.Translate(Vector2.right * SizeX);
            Coordinate.x += 1;
            return true;
        }
        if (Input.GetButtonDown("Up"))
        {
            if (Coordinate.y + 1 > Map.Instance.Rows)
                return false;

            this.transform.Translate(Vector2.up * SizeY);
            Coordinate.y += 1;
            return true;
        }
        if (Input.GetButtonDown("Down"))
        {
            if (Coordinate.y - 1 < 0)
                return false;

            this.transform.Translate(Vector2.down * SizeY);
            Coordinate.y -= 1;
            return true;
        }
        return false;
    }

    public void Disable()
    {
        this.enabled = false;
    }
    public void Enable()
    {
        this.enabled = true;
    }
}
