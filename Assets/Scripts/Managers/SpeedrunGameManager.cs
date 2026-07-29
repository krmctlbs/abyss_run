using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AbyssRun.Managers
{
    public enum GameState { Ready, Playing, GameOver, Victory }

    public class SpeedrunGameManager : MonoBehaviour
    {
        public static SpeedrunGameManager Instance { get; private set; }

        [Header("Game Modes & Rules")]
        [SerializeField] private float totalTimeSeconds = 60f;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float heightMultiplier = 1.0f;

        [Header("State & Events")]
        public UnityEvent<string> onTimerUpdated;
        public UnityEvent<int> onHeightUpdated;
        public UnityEvent<float, int, bool, bool> onGameEnd;

        private GameState currentState = GameState.Ready;
        private float remainingTime;
        private float startYPosition = 0f;
        private int maxHeightReached = 0;

        public GameState CurrentState => currentState;
        public int MaxHeightReached => maxHeightReached;
        public int BestMaxHeight { get; private set; }
        public float BestTime { get; private set; }
        public int WinCount { get; private set; }

        public string BestScoreDisplay
        {
            get
            {
                if (WinCount > 0 && BestTime < 999f) return $"BEST: {BestTime:F2} S ({WinCount}x)";
                if (BestMaxHeight > 0) return $"BEST: {BestMaxHeight} M";
                return "BEST: ---- M";
            }
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            BestMaxHeight = PlayerPrefs.GetInt("AbyssBestHeight", 0);
            BestTime = PlayerPrefs.GetFloat("AbyssBestTime", 999f);
            WinCount = PlayerPrefs.GetInt("AbyssWinCount", 0);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                startYPosition = playerTransform.position.y;
            }
            StartRun();
        }

        public void StartRun()
        {
            Time.timeScale = 1.0f;
            currentState = GameState.Ready;
            remainingTime = totalTimeSeconds;
            maxHeightReached = 0;

            int seconds = Mathf.FloorToInt(remainingTime);
            string timeFormatted = string.Format("{0:00}:00", seconds);
            onTimerUpdated?.Invoke(timeFormatted);
        }

        private void Update()
        {
            HandleRestartInput();

            if (currentState == GameState.Ready)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || 
                    Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift) || 
                    Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.05f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.05f)
                {
                    currentState = GameState.Playing;
                }
            }

            if (currentState != GameState.Playing) return;

            UpdateTimer();
            UpdateHeightTracker();
        }

        private void HandleRestartInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartRun();
            }
        }

        private void UpdateTimer()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                EndGame(false);
            }

            int seconds = Mathf.FloorToInt(remainingTime);
            int milliseconds = Mathf.FloorToInt((remainingTime - seconds) * 100f);
            string timeFormatted = string.Format("{0:00}:{1:00}", seconds, milliseconds);
            
            onTimerUpdated?.Invoke(timeFormatted);
        }

        private void UpdateHeightTracker()
        {
            if (playerTransform == null) return;

            float currentY = playerTransform.position.y - startYPosition;
            int currentMeters = Mathf.Max(0, Mathf.FloorToInt(currentY * heightMultiplier));

            if (currentMeters > maxHeightReached)
            {
                maxHeightReached = currentMeters;
                onHeightUpdated?.Invoke(maxHeightReached);
            }
        }

        public void TriggerVictory()
        {
            if (currentState == GameState.Playing || currentState == GameState.Ready)
            {
                EndGame(true);
            }
        }

        public void EndGame(bool victory)
        {
            if (currentState == GameState.GameOver || currentState == GameState.Victory) return;

            currentState = victory ? GameState.Victory : GameState.GameOver;
            float elapsedTime = totalTimeSeconds - remainingTime;
            bool isNewRecord = false;

            if (maxHeightReached > BestMaxHeight)
            {
                BestMaxHeight = maxHeightReached;
                PlayerPrefs.SetInt("AbyssBestHeight", BestMaxHeight);
                if (!victory && WinCount == 0) isNewRecord = true;
            }

            if (victory)
            {
                WinCount++;
                PlayerPrefs.SetInt("AbyssWinCount", WinCount);

                if (elapsedTime < BestTime)
                {
                    BestTime = elapsedTime;
                    PlayerPrefs.SetFloat("AbyssBestTime", BestTime);
                    isNewRecord = true;
                }
            }

            Time.timeScale = 0f;
            PlayerPrefs.Save();
            onGameEnd?.Invoke(elapsedTime, maxHeightReached, victory, isNewRecord);
        }

        public void RestartRun()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ResetAllRecords()
        {
            PlayerPrefs.DeleteKey("AbyssBestHeight");
            PlayerPrefs.DeleteKey("AbyssBestTime");
            PlayerPrefs.DeleteKey("AbyssWinCount");
            BestMaxHeight = 0;
            BestTime = 999f;
            WinCount = 0;
        }
    }
}
