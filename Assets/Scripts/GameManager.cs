using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameManager {
    [RuntimeInitializeOnLoadMethod]
    static void Init() {
        Debug.Log("GameManager initialized");
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // maybe it just doesnt call automatically on the game starting?
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log($"Scene loaded: {scene.name}");
        switch (scene.name) {
            case "Title":
                gameState = GameState.Title;
                foreach (Button b in Object.FindObjectsByType<Button>()) {
                    int playerCnt = int.Parse(b.gameObject.name[..1]);
                    b.OnPress = () => StartGame(playerCnt);
                }
                break;
            case "Game":
                gameState = GameState.Lottery;
                CreatePlayers();
                break;
        }
    }

    private static void StartGame(int playerCount) {
        Debug.Log($"Starting game with {playerCount} players");
        GameManager.playerCount = playerCount;
        SceneManager.LoadScene("Game");
    }

    private static void CreatePlayers() {
        if (SceneManager.GetActiveScene().name != "Game") return;
        GameObject playerTemplate = GameObject.Find("player");
        players.Add(playerTemplate);
        int cnt = playerCount;

        if (playerCount == 1) {
            cnt = 2;
            botPlaying = true;
        } else {
            botPlaying = false;
        }
        for (int i = 0; i < cnt; i++) {
            GameObject player;
            if (i == 0)
                player = playerTemplate;
            else
                player = Object.Instantiate(playerTemplate);
            player.transform.position += new Vector3(i * 6f, 0f);
            SpriteRenderer sr = player.transform.Find("guy/name")?.GetComponent<SpriteRenderer>();
            if (i == playerCount && botPlaying) {
                sr.sprite = spriteBank.GetSprite(0);
            } else {
                sr.sprite = spriteBank.GetSprite(i + 1);
            }
        }
        rollStart = Time.time + 3f;
    }

    public static void Update() {
        if (gameState == GameState.Lottery && Time.time > rollStart && rollStart != -1f) {
            foreach (Player p in playerControllers) {
                p.Roll();
            }
            rollStart = -1f;
            waitingForRollToFinish = true;
        } else if (waitingForRollToFinish) {
            bool finished = true;
            foreach (Player p in playerControllers) {
                if (p.isRolling)
                    finished = false;
            }
            if (finished) {
                waitingForRollToFinish = false;
                int highestScore = -1;
                int highestPlayer = -1;
                for (int i = 0; i < playerControllers.Count; i++) {
                    if (playerControllers[i].GetScore() > highestScore) {
                        highestScore = playerControllers[i].GetScore();
                        highestPlayer = i;
                    }
                }
                bool tie = false;
                for (int i = 0; i < playerControllers.Count; i++) {
                    if (playerControllers[i].GetScore() == highestScore && i != highestPlayer) {
                        tie = true;
                        break;
                    }
                }
                if (tie) {
                    rollStart = Time.time + 3f;
                    return;
                }
                selectedPlayer = highestPlayer;
                Debug.Log($"Player {selectedPlayer + 1} wins with {highestScore}");
                // move to stone phase
            }
        }
    }


    public enum GameState {
        Title,
        Lottery,
        Stone,
        GameOver,
    }

    private static float rollStart = -1f;
    private static bool waitingForRollToFinish = false;
    public static int playerCount = 1;
    public static int selectedPlayer = 0;
    public static bool botPlaying = false;
    public static GameState gameState = GameState.Title;
    private static List<GameObject> players = new();
    public static List<Player> playerControllers = new();
    public static List<Sprite> playerNames = new();
    public static SpriteBank spriteBank;
}