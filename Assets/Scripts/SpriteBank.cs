using System.Collections.Generic;
using UnityEngine;

// just a quick and ditry way to solve my problems cause i am on a time crunch
public class SpriteBank : MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(gameObject);
        GameManager.spriteBank = this;
    }

    public Sprite GetSprite(int index) {
        return spritesList[index];
    }

    public List<Sprite> spritesList = new();
}