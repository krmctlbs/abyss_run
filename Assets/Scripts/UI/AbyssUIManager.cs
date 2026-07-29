using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using AbyssRun.Player;
using AbyssRun.Managers;

namespace AbyssRun.UI
{
    public class AbyssUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AbyssPlayerController playerController;
        [SerializeField] private SpeedrunGameManager gameManager;

        [Header("HUD Elements")]
        [SerializeField] private TMP_Text altimeterText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private Image[] chargeDots;

        [Header("Dot Colors")]
        [SerializeField] private Color activeChargeColor = Color.red;
        [SerializeField] private Color spentChargeColor = Color.white;

        [Header("End Game Panels & Messages")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text gameOverMessageText;
        [SerializeField] private TMP_Text victoryMessageText;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button victoryRestartButton;
        [SerializeField] private float typewriterSpeed = 0.025f;

        private readonly string[] gameOverQuotes = new string[]
        {
            "\"Falls are merely new ground to spring from. Do not give up.\"\n— Abyss Oracle",
            "\"Ever tried. Ever failed. No matter. Try again. Fail again. Fail better.\"\n— Samuel Beckett",
            "\"Our greatest glory is not in never falling, but in rising every time we fall.\"\n— Confucius",
            "\"He who has a why to live can bear almost any how.\"\n— Friedrich Nietzsche"
        };

        private readonly string[] victoryQuotes = new string[]
        {
            "\"Even the deepest darkness eventually meets the light. Congratulations.\"\n— Abyss Oracle",
            "\"No tree, it is said, can grow to heaven unless its roots reach down to hell.\"\n— C.G. Jung",
            "\"It is not because things are difficult that we dare not venture. It is because we dare not venture that they are difficult.\"\n— Seneca",
            "\"Knowing yourself is the beginning of all wisdom.\"\n— Aristotle"
        };

        private void Start()
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<AbyssPlayerController>();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<SpeedrunGameManager>();

            if (gameManager != null)
            {
                gameManager.onTimerUpdated.AddListener(UpdateTimerText);
                gameManager.onGameEnd.AddListener(HandleGameEnd);

                if (bestScoreText != null)
                {
                    bestScoreText.text = gameManager.BestScoreDisplay;
                }
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            if (victoryRestartButton != null)
                victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        private void Update()
        {
            if (playerController == null) return;

            UpdateAltimeter();
            UpdateWallJumpCharges();
        }

        private void UpdateAltimeter()
        {
            if (altimeterText == null) return;

            int height = Mathf.FloorToInt(Mathf.Max(0f, playerController.transform.position.y));
            altimeterText.text = $"{height} M";
        }

        private void UpdateWallJumpCharges()
        {
            if (chargeDots == null || chargeDots.Length == 0) return;

            int charges = playerController.WallJumpCharges;

            for (int i = 0; i < chargeDots.Length; i++)
            {
                if (chargeDots[i] != null)
                {
                    chargeDots[i].color = (i < charges) ? activeChargeColor : spentChargeColor;
                }
            }
        }

        private void UpdateTimerText(string formattedTime)
        {
            if (timerText != null)
            {
                timerText.text = formattedTime;
            }
        }

        private void HandleGameEnd(float elapsedTime, int maxHeight, bool isVictory, bool isNewRecord)
        {
            if (bestScoreText != null && gameManager != null)
            {
                bestScoreText.text = gameManager.BestScoreDisplay;
            }

            string recordNotice = isNewRecord ? " NEW RECORD!" : "\n";

            if (isVictory)
            {
                if (victoryPanel != null)
                {
                    victoryPanel.SetActive(true);
                    string header = "YOU WON!";
                    string selectedQuote = victoryQuotes[Random.Range(0, victoryQuotes.Length)];
                    string stats = $"Completion Time: {elapsedTime:F2}s{recordNotice}Total Victories: {gameManager.WinCount}";
                    StartCoroutine(TypewriterEffect(header, selectedQuote, stats, victoryMessageText));
                }
            }
            else
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    string header = "TIME EXPIRED";
                    string selectedQuote = gameOverQuotes[Random.Range(0, gameOverQuotes.Length)];
                    string stats = $"Altitude Reached: {maxHeight} M{recordNotice} Best Record: {gameManager.BestMaxHeight} M";
                    StartCoroutine(TypewriterEffect(header, selectedQuote, stats, gameOverMessageText));
                }
                else if (victoryPanel != null)
                {
                    victoryPanel.SetActive(true);
                    string header = "TIME EXPIRED";
                    string selectedQuote = gameOverQuotes[Random.Range(0, gameOverQuotes.Length)];
                    string stats = $"Altitude Reached: {maxHeight} M\nBest Record: {gameManager.BestMaxHeight} M";
                    StartCoroutine(TypewriterEffect(header, selectedQuote, stats, victoryMessageText));
                }
            }
        }

        private IEnumerator TypewriterEffect(string header, string quoteWithAuthor, string stats, TMP_Text targetText)
        {
            if (targetText == null) yield break;

            string fullMessage = $"{header}\n\n{quoteWithAuthor}\n\n{stats}\n\n[PRESS R TO RETRY]";
            targetText.text = "";

            foreach (char character in fullMessage)
            {
                targetText.text += character;
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }
        }

        private void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartRun();
            }
        }
    }
}
