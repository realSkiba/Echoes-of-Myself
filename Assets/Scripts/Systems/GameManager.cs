using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverScreen;   // assign in Inspector

    bool isGameOver = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: persist across scenes
        // DontDestroyOnLoad(gameObject);

        if (gameOverScreen) gameOverScreen.SetActive(false);
    }

    void Update() {
        if (!isGameOver) return;

        // allow restart with R
        if (Input.GetKeyDown(KeyCode.R)) {
            // reset time scale in case we changed it
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GameOver() {
        if (isGameOver) return;
        isGameOver = true;

        // show UI
        if (gameOverScreen) gameOverScreen.SetActive(true);

        // stop time OR just stop movement – here we pause time
        Time.timeScale = 0f;

        // disable player control
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) {
            var pc = player.GetComponent<PlayerController>();
            if (pc) pc.enabled = false;
            var interactor = player.GetComponent<Interactor>();
            if (interactor) interactor.enabled = false;
            var echoRec = player.GetComponent<EchoRecorder>();
            if (echoRec) echoRec.enabled = false;
        }

        // optionally stop guard logic too
        foreach (var guard in FindObjectsOfType<GuardFSM>()) {
            guard.enabled = false;
            var agent = guard.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.isStopped = true;
        }

        Debug.Log("GAME OVER");
    }
}

