using UnityEngine;
using UnityEngine.UI;

namespace Arkanoid.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Text References")]
        public Text scoreText;
        public Text highScoreText;
        public Text livesText;
        public Text levelText;
        public Text powerUpText;

        private float powerUpPulseTimer = 0f;

        private void Update()
        {
            // Pulsate powerup text if active
            if (powerUpText != null && powerUpText.gameObject.activeSelf)
            {
                powerUpPulseTimer += Time.deltaTime * 6f;
                float scale = 1f + Mathf.Sin(powerUpPulseTimer) * 0.1f;
                powerUpText.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (powerUpText != null) powerUpText.gameObject.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score:D6}";
            }
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"HI-SCORE: {highScore:D6}";
            }
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                // Display as glowing paddle icons (e.g. "▲ ▲ ▲")
                string livesDisplay = "";
                for (int i = 0; i < lives; i++)
                {
                    livesDisplay += "▲ ";
                }
                livesText.text = $"LIVES: {livesDisplay.Trim()}";
            }
        }

        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"LEVEL: {level}";
            }
        }

        public void ShowPowerUpText(string type)
        {
            if (powerUpText != null)
            {
                powerUpText.gameObject.SetActive(true);
                powerUpText.text = $"- {type.ToUpper()} ACTIVE -";
                
                // Color match the powerup
                Color textColor = Color.white;
                switch (type)
                {
                    case "Expand": textColor = new Color(1f, 0.08f, 0.58f); break; // Pink
                    case "Laser": textColor = new Color(1f, 0.1f, 0.1f); break; // Red
                    case "Catch": textColor = new Color(0f, 1f, 0.5f); break; // Green
                    case "Slow": textColor = new Color(1f, 0.6f, 0f); break; // Orange
                    case "Pierce": textColor = new Color(1f, 0.9f, 0f); break; // Yellow
                }
                powerUpText.color = textColor;
                powerUpPulseTimer = 0f;
            }
        }

        public void HidePowerUpText()
        {
            if (powerUpText != null)
            {
                powerUpText.gameObject.SetActive(false);
            }
        }
    }
}
