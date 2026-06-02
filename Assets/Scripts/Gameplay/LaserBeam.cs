using UnityEngine;
using Arkanoid.Core;

namespace Arkanoid.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LaserBeam : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 15f;
        public int damage = 1;

        private void Awake()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.gravityScale = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            // Move upwards
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            // Destroy if off-screen top
            if (transform.position.y > 8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Brick brick = other.GetComponent<Brick>();
            if (brick != null)
            {
                brick.Hit(damage);
                SpawnImpactParticles();
                Destroy(gameObject);
            }
        }

        private void SpawnImpactParticles()
        {
            // Spawn 3 mini particles on impact
            for (int i = 0; i < 3; i++)
            {
                GameObject p = new GameObject("LaserImpact");
                p.transform.position = transform.position;
                p.transform.localScale = new Vector3(0.08f, 0.08f, 1f);

                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                Texture2D tex = Texture2D.whiteTexture;
                sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f), tex.width);
                sr.color = Color.red;

                Rigidbody2D rb = p.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0.5f;

                float angle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
                float force = Random.Range(2f, 4f);
                rb.linearVelocity = new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * force; // bounce slightly back

                p.AddComponent<DebrisDestroyer>();
            }
        }
    }
}
