using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class Player : MonoBehaviour {
    void Start() {
        ticket = transform.Find("guy/number").GetComponent<SpriteRenderer>();
    }

    public void Roll() {
        rollTimer = Time.time + Random.Range(1.5f, 3f);
        isRolling = true;
    }

    void Update() {
        if (isRolling && Time.time < rollTimer) {
            ticket.sprite = sprites[Random.Range(0, sprites.Count)];
        }
        if (isRolling && Time.time > rollTimer) {
            isRolling = false;
            numRolled = Random.Range(0, sprites.Count);
            ticket.sprite = sprites[numRolled];
        }
    }

    public int GetScore() => numRolled;

    private float rollTimer = -1f;
    private int numRolled = -1;
    public bool isRolling = false;
    public int hp = 100;
    public int maxHp = 100;
    private SpriteRenderer ticket;
    public List<Sprite> sprites = new();
}