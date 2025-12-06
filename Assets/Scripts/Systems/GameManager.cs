using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverScreen;

    bool isGameOver = false;

    void Awake()
    {
        // simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameOverScreen) gameOverScreen.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver) return;

        // Restart on R
        if (Input.GetKeyDown(KeyCode.R))
        {
            // unpause
            Time.timeScale = 1f;

            // shut down any running network session
            if (NetworkManager.Singleton != null &&
                (NetworkManager.Singleton.IsHost ||
                 NetworkManager.Singleton.IsServer ||
                 NetworkManager.Singleton.IsClient))
            {
                NetworkManager.Singleton.Shutdown();
            }

            // reload scene fresh
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverScreen) gameOverScreen.SetActive(true);
        Time.timeScale = 0f;

        // disable player controls
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            var netPc = player.GetComponent<NetPlayerController>();
            if (netPc) netPc.enabled = false;

            var interactor = player.GetComponent<Interactor>();
            if (interactor) interactor.enabled = false;

            var echoRec = player.GetComponent<EchoRecorder>();
            if (echoRec) echoRec.enabled = false;
        }

        // stop guards
        foreach (var guard in FindObjectsOfType<GuardFSM>())
        {
            guard.enabled = false;
            var agent = guard.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.isStopped = true;
        }

        Debug.Log("GAME OVER");
    }
}
