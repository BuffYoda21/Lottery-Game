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
                attackBar = GameObject.Find("attackBar");
                barIndicator = attackBar.transform.Find("indicator");
                barIndicatorHome = barIndicator.position;
                attackBar.SetActive(false);
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
            players.Add(player);
            player.transform.position += new Vector3(i * 6f, 0f);
            SpriteRenderer sr = player.transform.Find("guy/name")?.GetComponent<SpriteRenderer>();
            if (i == playerCount && botPlaying) {
                sr.sprite = spriteBank.GetSprite(0);
            } else {
                sr.sprite = spriteBank.GetSprite(i + 1);
            }
        }
        foreach (GameObject p in players)
            playerControllers.Add(p.GetComponent<Player>());
        Debug.Log($"{playerControllers.Count} players created");
        rollStart = Time.time + 3f;
        if (botPlaying) playerCount++;
    }

    public static void Update() {
        if (gameState == GameState.Lottery && Time.time > rollStart && rollStart != -1f) {
            stoneSetup = false;
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
                int tiedPlayer1;
                int tiedPlayer2;
                for (int i = 0; i < playerControllers.Count; i++) {
                    if (playerControllers[i].GetScore() == highestScore && i != highestPlayer) {
                        tie = true;
                        tiedPlayer1 = highestPlayer;
                        tiedPlayer2 = i;
                        Debug.Log($"Tie, rerolling...");
                        Debug.Log($"Player {tiedPlayer1 + 1} and {tiedPlayer2 + 1} tied with {highestScore}");
                        break;
                    }
                }
                if (tie) {
                    rollStart = Time.time + 3f;
                    return;
                }
                selectedPlayer = highestPlayer;
                Debug.Log($"Player {selectedPlayer + 1} wins with {highestScore}");
                gameState = GameState.Stone;
                stoneStart = Time.time + 3f;
            }
        } else if (gameState == GameState.Stone && Time.time > stoneStart && stoneStart != -1f) {
            Debug.Log("Setting up stone");
            stoneStart = -1f;
            victim = Object.Instantiate(players[selectedPlayer], new Vector3(9f, 0f), Quaternion.identity);
            victim.transform.Find("guy/number")?.gameObject.SetActive(false);
            attackerName = Object.Instantiate(victim, new Vector3(-9f, 0f), Quaternion.identity)?.transform.Find("guy/name")?.GetComponent<SpriteRenderer>();
            foreach (GameObject p in players)
                p.SetActive(false);
            attackBar.SetActive(true);
            stoneSetup = true;
        } else if (gameState == GameState.Stone && stoneSetup) {
            if (turn >= playerCount) {
                turn = -1;
                turnFinished = true;
                Debug.Log("All turns finished");
                gameState = GameState.Lottery;
                stoneSetup = false;
                foreach (GameObject p in players)
                    p.SetActive(true);
                attackBar.SetActive(false);
                barIndicator.position = barIndicatorHome;
                Object.Destroy(victim);
                Object.Destroy(attackerName);
                rollStart = Time.time + 3f;
                ResetSoft();
                return;
            }
            if (turnFinished) {
                turnFinished = false;
                attacked = false;
                turn++;
                Debug.Log($"PlayerCount: {playerCount}");
                if (turn >= playerCount) return;
                if (turn == selectedPlayer) {
                    turnFinished = true;
                    Debug.Log($"Skipping player {turn + 1}");
                    return;
                }
                Debug.Log($"Player {turn + 1} turn");
                attackerName.sprite = turn + 1 == 2 && botPlaying ? spriteBank.GetSprite(0) : spriteBank.GetSprite(turn + 1);
                waitingForPlayer = true;
            }
            if (!attacked && waitingForPlayer && ((Input.GetKeyDown(KeyCode.Space) && (!botPlaying || turn != 1)) || (botPlaying && turn == 1))) {
                waitingForPlayer = false;
                barMoveStartTime = Time.time;
                barMoveEndTime = Time.time + Random.Range(0.5f, 2f);
                barIndicator.position = barIndicatorHome;
                if (botPlaying && turn == 1)
                    botBarStopTime = Time.time + Random.Range(0f, barMoveEndTime - barMoveStartTime);
            } else if (!attacked && !waitingForPlayer && Time.time < barMoveEndTime && barMoveEndTime != -1f) {
                barIndicator.transform.position = Vector3.Lerp(barIndicatorHome, barIndicatorHome + new Vector3(7.5f, 0f), 1f - ((barMoveEndTime - Time.time) / (barMoveEndTime - barMoveStartTime)));
                if ((Input.GetKeyDown(KeyCode.Space) && (!botPlaying || turn != 1)) || (botPlaying && turn == 1 && Time.time > botBarStopTime)) {
                    waitingForPlayer = true;
                    attacked = true;
                    float distanceFromCenter = System.Math.Abs(barIndicator.localPosition.x);
                    int dmg = (int)(20f - distanceFromCenter * 5f);
                    if (dmg < 0) dmg = 0;
                    playerControllers[selectedPlayer].hp -= dmg;
                    Debug.Log($"Player {selectedPlayer + 1} took {dmg} damage");
                    if (playerControllers[selectedPlayer].hp <= 0) {
                        playerControllers[selectedPlayer].hp = 0;
                        Debug.Log($"Player {selectedPlayer + 1} is dead");
                        gameState = GameState.GameOver;
                        SceneManager.LoadScene("Title"); // change this to game over scene when implemented
                        // load game over scene
                        Reset();
                        return;
                    }
                    turnEndTime = Time.time + 2f;
                }
            } else if (!waitingForPlayer && Time.time > barMoveEndTime && turnEndTime == -1f) {
                turnEndTime = Time.time + 2f;
                waitingForPlayer = true;
            } else if (waitingForPlayer && Time.time > turnEndTime && turnEndTime != -1f) {
                turnFinished = true;
                waitingForPlayer = false;
                barMoveStartTime = -1f;
                barMoveEndTime = -1f;
                turnEndTime = -1f;
            }
        }
    }

    private static void Reset() {
        rollStart = -1f;
        stoneStart = -1f;
        turn = -1;
        turnFinished = true;
        waitingForPlayer = true;
        attacked = false;
        barMoveStartTime = -1f;
        barMoveEndTime = -1f;
        turnEndTime = -1f;
        botBarStopTime = -1f;
        stoneSetup = false;
        waitingForRollToFinish = false;
        playerCount = 1;
        selectedPlayer = 0;
        botPlaying = false;
        gameState = GameState.Title;
        players.Clear();
        playerControllers.Clear();
        playerNames.Clear();
        attackerName = null;
        attackBar = null;
        barIndicator = null;
    }

    private static void ResetSoft() {
        stoneStart = -1f;
        turn = -1;
        turnFinished = true;
        waitingForPlayer = true;
        attacked = false;
        barMoveStartTime = -1f;
        barMoveEndTime = -1f;
        botBarStopTime = -1f;
        turnEndTime = -1f;
        stoneSetup = false;
    }


    public enum GameState {
        Title,
        Lottery,
        Stone,
        GameOver,
    }

    private static float rollStart = -1f;
    private static float stoneStart = -1f;
    private static int turn = -1;
    private static bool turnFinished = true;
    private static bool waitingForPlayer = true;
    private static bool attacked = false;
    private static float barMoveEndTime = -1f;
    private static float barMoveStartTime = -1f;
    private static float turnEndTime = -1f;
    private static float botBarStopTime = -1f;
    private static Vector3 barIndicatorHome = new(0f, 0f);
    private static bool waitingForRollToFinish = false;
    public static int playerCount = 1;
    public static int selectedPlayer = 0;
    public static bool botPlaying = false;
    private static bool stoneSetup = false;
    public static GameState gameState = GameState.Title;
    private static List<GameObject> players = new();
    public static List<Player> playerControllers = new();
    public static List<Sprite> playerNames = new();
    private static GameObject victim;
    private static SpriteRenderer attackerName;
    private static GameObject attackBar;
    private static Transform barIndicator;
    public static SpriteBank spriteBank;
}