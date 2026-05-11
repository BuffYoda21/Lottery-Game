using System.Collections.Generic;
using UnityEngine;

// just a quick and ditry way to solve my problems cause i am on a time crunch
public class SpriteBank : MonoBehaviour {
    void Awake() {
        if (GameManager.spriteBank != null) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        GameManager.spriteBank = this;
    }

    // not related to sprite bank but time crunch and i needed it subscribed to the update cycle
    void Update() {
        GameManager.Update();
    }

    public Sprite GetSprite(int index) {
        return spritesList[index];
    }

    public List<Sprite> spritesList = new();
}