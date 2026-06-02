using UnityEngine;
using UnityEngine.UI;
using Arkanoid.Core;

namespace Arkanoid.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject pausePanel;
        public GameObject gameOverPanel;
        public GameObject levelCompletePanel;
        public GameObject victoryPanel;

        [Header("Main Menu Text")]
        public Text mainMenuHighScoreText;

        [Header("Game Over Text")]
        public Text gameOverScoreText;
        public Text gameOverHighScoreText;

        [Header("Level Complete Text")]
        public Text levelCompleteTitleText;

        [Header("Victory Text")]
        public Text victoryScoreText;
        public Text victoryHighScoreText;

        [Header("Buttons")]
        public Button playButton;
        public Button resumeButton;
        public Button retryButton;
        public Button nextLevelButton;
        public Button victoryPlayAgainButton;
        public Button exitButton;
        public Button mainMenuButton;

        private int pendingNextLevelIndex = 0;

        private void Start()
        {
            // Bind buttons programmatically to ensure robust linkage
            if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
            if (retryButton != null) retryButton.onClick.AddListener(OnPlayPressed);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelPressed);
            if (victoryPlayAgainButton != null) victoryPlayAgainButton.onClick.AddListener(OnPlayPressed);
            if (exitButton != null) exitButton.onClick.AddListener(OnExitPressed);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuPressed);
        }

        public void HideAll()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        public void ShowMainMenu(int highScore)
        {
            HideAll();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (mainMenuHighScoreText != null)
            {
                mainMenuHighScoreText.text = $"HI-SCORE: {highScore:D6}";
            }
        }

        public void ShowPauseMenu()
        {
            HideAll();
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void ShowGameOver(int score, int highScore)
        {
            HideAll();
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverScoreText != null) gameOverScoreText.text = $"FINAL SCORE: {score:D6}";
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"HI-SCORE: {highScore:D6}";
        }

        public void ShowLevelComplete(int nextLevelIndex)
        {
            HideAll();
            pendingNextLevelIndex = nextLevelIndex;
            if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
            if (levelCompleteTitleText != null)
            {
                levelCompleteTitleText.text = $"LEVEL {nextLevelIndex} COMPLETED";
            }
        }

        public void ShowVictory(int score, int highScore)
        {
            HideAll();
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryScoreText != null) victoryScoreText.text = $"FINAL SCORE: {score:D6}";
            if (victoryHighScoreText != null) victoryHighScoreText.text = $"HI-SCORE: {highScore:D6}";
        }

        // Button Listeners
        private void OnPlayPressed()
        {
            GameManager.Instance.StartNewGame();
        }

        private void OnResumePressed()
        {
            GameManager.Instance.TogglePause();
        }

        private void OnNextLevelPressed()
        {
            GameManager.Instance.LoadLevel(pendingNextLevelIndex);
        }

        private void OnExitPressed()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnMainMenuPressed()
        {
            GameManager.Instance.ShowMainMenu();
        }
    }
}
