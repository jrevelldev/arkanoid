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
        private bool isSelecting = false;
        private GameObject lastSelectedObject = null;
        private string lastOriginalText = null;
        private Text lastTextComponent = null;

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

        private void Update()
        {
            // If any menu panel is active and selection is lost, deselect/re-select appropriate button
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null &&
                !isSelecting)
            {
                if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy) SelectButton(playButton);
                else if (pausePanel != null && pausePanel.activeInHierarchy) SelectButton(resumeButton);
                else if (gameOverPanel != null && gameOverPanel.activeInHierarchy) SelectButton(retryButton);
                else if (levelCompletePanel != null && levelCompletePanel.activeInHierarchy) SelectButton(nextLevelButton);
                else if (victoryPanel != null && victoryPanel.activeInHierarchy) SelectButton(victoryPlayAgainButton);
            }

            // Handle visual > < selection indicators
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                GameObject currentSel = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (currentSel != lastSelectedObject)
                {
                    // Restore previous
                    if (lastTextComponent != null && !string.IsNullOrEmpty(lastOriginalText))
                    {
                        lastTextComponent.text = lastOriginalText;
                    }
                    
                    lastSelectedObject = null;
                    lastOriginalText = null;
                    lastTextComponent = null;

                    // Decorate new
                    if (currentSel != null)
                    {
                        Text txt = currentSel.GetComponentInChildren<Text>();
                        if (txt != null)
                        {
                            lastSelectedObject = currentSel;
                            lastTextComponent = txt;
                            lastOriginalText = txt.text;
                            txt.text = "> " + lastOriginalText + " <";
                        }
                    }
                }
            }
        }

        public void HideAll()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            // Clear EventSystem selection to avoid accidental space/enter triggers in gameplay
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void SelectButton(Button button)
        {
            if (button != null && !isSelecting)
            {
                StartCoroutine(SelectButtonRoutine(button));
            }
        }

        private System.Collections.IEnumerator SelectButtonRoutine(Button button)
        {
            isSelecting = true;
            yield return null; // Wait for end of frame / next frame so UI can activate
            if (button != null && button.gameObject.activeInHierarchy)
            {
                button.Select();
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
            }
            isSelecting = false;
        }

        public void ShowMainMenu(int highScore)
        {
            HideAll();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (mainMenuHighScoreText != null)
            {
                mainMenuHighScoreText.text = $"HI-SCORE: {highScore:D6}";
            }
            SelectButton(playButton);
        }

        public void ShowPauseMenu()
        {
            HideAll();
            if (pausePanel != null) pausePanel.SetActive(true);
            SelectButton(resumeButton);
        }

        public void ShowGameOver(int score, int highScore)
        {
            HideAll();
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverScoreText != null) gameOverScoreText.text = $"FINAL SCORE: {score:D6}";
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"HI-SCORE: {highScore:D6}";
            SelectButton(retryButton);
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
            SelectButton(nextLevelButton);
        }

        public void ShowVictory(int score, int highScore)
        {
            HideAll();
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryScoreText != null) victoryScoreText.text = $"FINAL SCORE: {score:D6}";
            if (victoryHighScoreText != null) victoryHighScoreText.text = $"HI-SCORE: {highScore:D6}";
            SelectButton(victoryPlayAgainButton);
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
