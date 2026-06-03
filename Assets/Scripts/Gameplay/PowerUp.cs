using UnityEngine;
using Arkanoid.Core;

namespace Arkanoid.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PowerUp : MonoBehaviour
    {
        [Header("Settings")]
        public float fallSpeed = 2.5f;

        private string powerUpType = "Expand";
        private TextMesh labelMesh;

        private void Awake()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.gravityScale = 0f;
        }

        public void Initialize(string type)
        {
            this.powerUpType = type;

            // Setup color based on type
            Color puColor = Color.white;
            string letter = "E";

            switch (type)
            {
                case "Expand":
                    puColor = new Color(1f, 0.08f, 0.58f); // Neon pink
                    letter = "E";
                    break;
                case "Laser":
                    puColor = new Color(1f, 0.1f, 0.1f); // Neon red
                    letter = "L";
                    break;
                case "Catch":
                    puColor = new Color(0f, 1f, 0.5f); // Neon green
                    letter = "C";
                    break;
                case "Triple":
                    puColor = new Color(0f, 0.8f, 1f); // Neon cyan
                    letter = "T";
                    break;
                case "Slow":
                    puColor = new Color(1f, 0.6f, 0f); // Neon orange
                    letter = "A";
                    break;
                case "Pierce":
                    puColor = new Color(1f, 0.9f, 0f); // Neon yellow
                    letter = "P";
                    break;
                case "Life":
                    puColor = new Color(1f, 0.84f, 0f); // Gold
                    letter = "1UP";
                    break;
            }

            // Color the sprite
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = puColor;
            }

            // Create a child TextMesh for displaying the letter
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.parent = transform;
            labelObj.transform.localPosition = Vector3.zero;

            labelMesh = labelObj.AddComponent<TextMesh>();
            labelMesh.text = letter;
            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;
            labelMesh.fontSize = 32;
            labelMesh.characterSize = 0.08f;
            labelMesh.color = Color.white;
            labelMesh.fontStyle = FontStyle.Bold;

            // Keep text in front of capsule
            labelObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            // Move downwards
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            // Self destroy if falls off-screen
            if (transform.position.y < -8.5f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PaddleController paddle = other.GetComponent<PaddleController>();
            if (paddle != null)
            {
                GameManager.Instance.ActivatePowerUp(powerUpType);
                Destroy(gameObject);
            }
        }
    }
}
