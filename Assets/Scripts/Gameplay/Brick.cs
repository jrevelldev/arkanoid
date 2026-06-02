using UnityEngine;
using System.Collections.Generic;
using Arkanoid.Core;

namespace Arkanoid.Gameplay
{
    public class Brick : MonoBehaviour
    {
        [Header("References")]
        public PowerUp powerUpPrefab;

        private int maxHits = 1;
        private int currentHits = 0;
        private int points = 50;
        private Color baseColor = Color.white;
        private bool isUnbreakable = false;

        private SpriteRenderer spriteRenderer;
        private Coroutine flashCoroutine;

        public bool IsUnbreakable => isUnbreakable;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(int maxHits, int points, Color baseColor, bool isUnbreakable)
        {
            this.maxHits = maxHits;
            this.currentHits = maxHits;
            this.points = points;
            this.baseColor = baseColor;
            this.isUnbreakable = isUnbreakable;

            UpdateBrickAppearance();
        }

        public void Hit(int damage)
        {
            if (isUnbreakable)
            {
                // Unbreakable bricks play a hit sound but take no damage
                SoundManager.Instance.PlayBrickHit();
                TriggerFlash();
                return;
            }

            currentHits -= damage;

            if (currentHits <= 0)
            {
                Break();
            }
            else
            {
                SoundManager.Instance.PlayBrickHit();
                TriggerFlash();
            }
        }

        private void Break()
        {
            SoundManager.Instance.PlayBrickBreak();

            // Notify game manager
            GameManager.Instance.AddScore(points, transform.position);

            // Spawn visual juice
            SpawnDebris();

            // Try to roll for powerup drop
            TrySpawnPowerUp();

            // Remove from level manager and check level completion immediately
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RemoveBrick(this);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckLevelCompletion();
            }

            // Destroy game object
            Destroy(gameObject);
        }

        private void TriggerFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
            yield return new WaitForSeconds(0.06f);
            UpdateBrickAppearance();
        }

        private void UpdateBrickAppearance()
        {
            if (spriteRenderer == null) return;

            if (isUnbreakable)
            {
                spriteRenderer.color = baseColor;
                return;
            }

            // Darken color depending on remaining health
            float healthRatio = (float)currentHits / maxHits;
            
            // Neon colors look best when bright, so we blend with a slightly darker version
            Color targetColor = Color.Lerp(baseColor * 0.4f, baseColor, healthRatio);
            // Ensure full alpha
            targetColor.a = 1.0f;
            
            spriteRenderer.color = targetColor;
        }

        private void SpawnDebris()
        {
            // Spawn 6 to 10 debris particles programmatically
            int count = Random.Range(6, 11);
            for (int i = 0; i < count; i++)
            {
                GameObject debris = new GameObject("Debris");
                debris.transform.position = transform.position;
                debris.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
                Texture2D tex = Texture2D.whiteTexture;
                sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f), tex.width);
                sr.color = baseColor;

                // Simple gravity & motion controller
                Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1.5f;
                
                // Explode in random directions
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float force = Random.Range(3f, 7f);
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * force;

                // Self destroy after 1 second and fade
                debris.AddComponent<DebrisDestroyer>();
            }
        }

        private void TrySpawnPowerUp()
        {
            // 15% chance to drop powerup
            if (Random.value <= 0.15f)
            {
                // Select random type
                string[] types = { "Expand", "Laser", "Catch", "Triple", "Slow", "Pierce", "Life" };
                string randomType = types[Random.Range(0, types.Length)];

                if (powerUpPrefab != null)
                {
                    PowerUp powerup = Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
                    powerup.Initialize(randomType);
                }
                else
                {
                    // Fallback: Create powerup programmatically
                    GameObject puObj = new GameObject("ProceduralPowerUp");
                    puObj.transform.position = transform.position;
                    
                    SpriteRenderer sr = puObj.AddComponent<SpriteRenderer>();
                    Texture2D tex = Texture2D.whiteTexture;
                    sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f), tex.width);
                    
                    // Capsule size
                    puObj.transform.localScale = new Vector3(0.35f, 0.6f, 1f);

                    CapsuleCollider2D col = puObj.AddComponent<CapsuleCollider2D>();
                    col.isTrigger = true;

                    PowerUp powerup = puObj.AddComponent<PowerUp>();
                    powerup.Initialize(randomType);
                }
            }
        }
    }

    // Helper component to clean up debris and fade out
    public class DebrisDestroyer : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float lifeTime = 0.8f;
        private float elapsed = 0f;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= lifeTime)
            {
                Destroy(gameObject);
            }
            else if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1.0f, 0.0f, elapsed / lifeTime);
                sr.color = c;
            }
        }
    }
}
