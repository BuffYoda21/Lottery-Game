using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Button : MonoBehaviour {
    void Start() {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (isPressed && Input.GetMouseButtonUp(0)) {
            sr.color = idleColor;
            isPressed = false;
            OnPress();
        }
    }

    void OnMouseEnter() {
        sr.color = hoverColor;
    }

    void OnMouseExit() {
        sr.color = idleColor;
        isPressed = false;
    }

    void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            sr.color = pressedColor;
            isPressed = true;
        }
    }

    private SpriteRenderer sr;

    private bool isPressed = false;
    public Color32 idleColor = new Color32(255, 255, 255, 255);
    public Color32 hoverColor = new Color32(255, 255, 255, 255);
    public Color32 pressedColor = new Color32(255, 255, 255, 255);

    public Action OnPress = () => {
        Debug.LogWarning("Button pressed but no event was set");
    };
}
