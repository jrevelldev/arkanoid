using UnityEngine;
using System.Collections.Generic;
using Arkanoid.Gameplay;
using Arkanoid.UI;
using Arkanoid.Effects;

namespace Arkanoid.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelCompleted,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        public int startingLives = 3;
        public float powerUpDuration = 10f;

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;

        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public int Lives { get; private set; } = 3;
        public int CurrentLevel { get; private set; } = 0;

        [Header("References")]
        public PaddleController paddle;
        public BallController ballPrefab;
        public HUDController hudController;
        public MenuController menuController;

        private List<BallController> activeBalls = new List<BallController>();
        private float powerUpTimer = 0f;
        private string activePowerUpType = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                HighScore = PlayerPrefs.GetInt("HighScore", 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
#if ENABLE_INPUT_SYSTEM
                bool pausePressed = (UnityEngine.InputSystem.Keyboard.current != null && 
                                    (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame || 
                                     UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame));
#else
                bool pausePressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
                if (pausePressed)
                {
                    TogglePause();
                }

                if (powerUpTimer > 0f)
                {
                    powerUpTimer -= Time.deltaTime;
                    if (powerUpTimer <= 0f)
                    {
                        DeactivatePowerUp();
                    }
                }
            }
        }

        public void ShowMainMenu()
        {
            currentState = GameState.MainMenu;
            Time.timeScale = 0f;
            if (menuController != null) menuController.ShowMainMenu(HighScore);
            if (hudController != null) hudController.Hide();
            ClearActiveGameplayElements();
        }

        public void StartNewGame()
        {
            Score = 0;
            Lives = startingLives;
            CurrentLevel = 0;
            Time.timeScale = 1f;

            if (hudController != null)
            {
                hudController.Show();
                hudController.UpdateScore(Score);
                hudController.UpdateHighScore(HighScore);
                hudController.UpdateLives(Lives);
                hudController.UpdateLevel(CurrentLevel + 1);
            }

            if (menuController != null) menuController.HideAll();
            LoadLevel(CurrentLevel);
        }

        public void LoadLevel(int levelIndex)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            if (menuController != null) menuController.HideAll();
            DeactivatePowerUp();
            ClearActiveGameplayElements();

            // Setup paddle position
            if (paddle != null)
            {
                paddle.transform.position = new Vector3(0, -6.5f, 0);
                paddle.ResetPaddle();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SpawnLevel(levelIndex);
            }
            SpawnBallOnPaddle();
            if (hudController != null) hudController.UpdateLevel(levelIndex + 1);
        }

        public void SpawnBallOnPaddle()
        {
            if (paddle == null || ballPrefab == null) return;
            BallController newBall = Instantiate(ballPrefab);
            newBall.transform.position = paddle.transform.position + Vector3.up * 0.5f;
            newBall.SetOnPaddle(paddle);
            activeBalls.Add(newBall);
        }

        public void AddBall(BallController ball)
        {
            if (ball != null && !activeBalls.Contains(ball))
                activeBalls.Add(ball);
        }

        public void RemoveBall(BallController ball)
        {
            activeBalls.Remove(ball);
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }

            if (activeBalls.Count == 0 && currentState == GameState.Playing)
            {
                LoseLife();
            }
        }

        private void LoseLife()
        {
            Lives--;
            if (hudController != null) hudController.UpdateLives(Lives);
            SoundManager.Instance.PlayDeath();
            if (EffectsManager.Instance != null) EffectsManager.Instance.ShakeCamera(0.3f, 0.4f);

            if (paddle != null)
            {
                paddle.ResetPaddle();
            }

            if (Lives > 0)
            {
                SpawnBallOnPaddle();
            }
            else
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
            SoundManager.Instance.PlayLoss();

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
            }

            if (menuController != null) menuController.ShowGameOver(Score, HighScore);
        }

        public void AddScore(int points, Vector3 position)
        {
            Score += points;
            if (hudController != null) hudController.UpdateScore(Score);
            if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnFloatingText($"+{points}", position);

            if (Score > HighScore)
            {
                HighScore = Score;
                if (hudController != null) hudController.UpdateHighScore(HighScore);
            }
        }

        public void CheckLevelCompletion()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.GetBreakableBricksCount() == 0)
            {
                StartCoroutine(LevelCompletedRoutine());
            }
        }

        private System.Collections.IEnumerator LevelCompletedRoutine()
        {
            // Lock state to LevelCompleted so no lives are lost, but keep time flowing
            currentState = GameState.LevelCompleted;

            // Wait 1.5 seconds while the ball continues its path
            yield return new WaitForSeconds(1.5f);

            Time.timeScale = 0f;
            SoundManager.Instance.PlayWin();

            CurrentLevel++;
            if (LevelManager.Instance != null && CurrentLevel < LevelManager.Instance.TotalLevels)
            {
                if (menuController != null) menuController.ShowLevelComplete(CurrentLevel);
            }
            else
            {
                TriggerVictory();
            }
        }

        private void TriggerVictory()
        {
            currentState = GameState.Victory;
            Time.timeScale = 0f;
            SoundManager.Instance.PlayWin();

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
            }

            if (menuController != null) menuController.ShowVictory(Score, HighScore);
        }

        public void ActivatePowerUp(string type)
        {
            SoundManager.Instance.PlayPowerUp();
            activePowerUpType = type;
            
            // For instantaneous powerups (Triple, Life), don't set a duration timer
            if (type == "Triple")
            {
                SplitBalls();
                if (hudController != null) hudController.HidePowerUpText();
                activePowerUpType = "";
                return;
            }
            else if (type == "Life")
            {
                Lives++;
                if (hudController != null) hudController.UpdateLives(Lives);
                if (hudController != null) hudController.HidePowerUpText();
                activePowerUpType = "";
                return;
            }

            powerUpTimer = powerUpDuration;
            if (hudController != null) hudController.ShowPowerUpText(type);

            switch (type)
            {
                case "Expand":
                    if (paddle != null) paddle.SetExpandMode(true);
                    break;
                case "Laser":
                    if (paddle != null) paddle.SetLaserMode(true);
                    break;
                case "Catch":
                    if (paddle != null) paddle.SetCatchMode(true);
                    break;
                case "Slow":
                    SlowDownBalls();
                    break;
                case "Pierce":
                    SetPierceMode(true);
                    break;
            }
        }

        private void DeactivatePowerUp()
        {
            if (hudController != null) hudController.HidePowerUpText();
            if (paddle != null)
            {
                paddle.SetExpandMode(false);
                paddle.SetLaserMode(false);
                paddle.SetCatchMode(false);
            }
            SetPierceMode(false);
            activePowerUpType = "";
            powerUpTimer = 0f;
        }

        private void SplitBalls()
        {
            List<BallController> currentBalls = new List<BallController>(activeBalls);
            foreach (var b in currentBalls)
            {
                if (b == null) continue;
                // Spawn 2 more balls from each current ball
                for (int i = 0; i < 2; i++)
                {
                    BallController ballCopy = Instantiate(ballPrefab);
                    ballCopy.transform.position = b.transform.position;
                    // Add slight random velocity variance
                    Vector2 baseVel = b.Velocity;
                    if (baseVel.magnitude < 0.1f) baseVel = Vector2.up * 6f;
                    float angle = (i == 0 ? 15f : -15f) * Mathf.Deg2Rad;
                    Vector2 newVel = new Vector2(
                        baseVel.x * Mathf.Cos(angle) - baseVel.y * Mathf.Sin(angle),
                        baseVel.x * Mathf.Sin(angle) + baseVel.y * Mathf.Cos(angle)
                    ).normalized * baseVel.magnitude;
                    
                    ballCopy.Launch(newVel);
                    activeBalls.Add(ballCopy);
                }
            }
        }

        private void SlowDownBalls()
        {
            foreach (var b in activeBalls)
            {
                if (b != null) b.SlowDown();
            }
        }

        private void SetPierceMode(bool active)
        {
            foreach (var b in activeBalls)
            {
                if (b != null) b.SetPierceMode(active);
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
                if (menuController != null) menuController.ShowPauseMenu();
            }
            else if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1f;
                if (menuController != null) menuController.HideAll();
            }
        }

        private void ClearActiveGameplayElements()
        {
            // Destroy active balls
            foreach (var ball in activeBalls)
            {
                if (ball != null) Destroy(ball.gameObject);
            }
            activeBalls.Clear();

            // Destroy active falling powerups
            PowerUp[] powerups = FindObjectsOfType<PowerUp>();
            foreach (var pu in powerups)
            {
                Destroy(pu.gameObject);
            }

            // Destroy active lasers
            LaserBeam[] lasers = FindObjectsOfType<LaserBeam>();
            foreach (var laser in lasers)
            {
                Destroy(laser.gameObject);
            }

            // Clear level bricks
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ClearBricks();
            }
        }
    }
}
