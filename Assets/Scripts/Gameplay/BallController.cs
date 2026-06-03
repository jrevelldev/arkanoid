using UnityEngine;
using Arkanoid.Core;

namespace Arkanoid.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour
    {
        [Header("Speed Settings")]
        public float baseSpeed = 7f;
        public float maxSpeed = 14f;
        public float speedIncrement = 0.15f;

        [Header("Visual Elements")]
        public TrailRenderer trailRenderer;
        public Color normalColor = Color.white;
        public Color fireballColor = new Color(1f, 0.3f, 0f); // Bright neon orange

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private PaddleController attachedPaddle;
        
        private float currentSpeed;
        private bool isOnPaddle = true;
        private bool isPierceMode = false;
        private Vector2 lastFrameVelocity;

        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup rigid body settings for perfect arcade physics
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            currentSpeed = baseSpeed;
        }

        private void Start()
        {
            SetupTrail();
        }

        private void SetupTrail()
        {
            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
            }

            if (trailRenderer != null)
            {
                trailRenderer.startWidth = 0.25f;
                trailRenderer.endWidth = 0.0f;
                trailRenderer.time = 0.2f;
                
                // Neon glow gradient
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0f, 0.9f, 1f), 0.5f), new GradientColorKey(new Color(0.5f, 0f, 1f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
            }
        }

        private void Update()
        {
            if (isOnPaddle && attachedPaddle != null)
            {
                // Lock to paddle position
                transform.position = attachedPaddle.transform.position + Vector3.up * 0.45f;

                // Launch if player presses launch buttons and state is Playing
#if ENABLE_INPUT_SYSTEM
                bool launchPressed = (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) || 
                                     (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame);
#else
                bool launchPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
                if (GameManager.Instance.CurrentState == GameState.Playing && launchPressed)
                {
                    LaunchFromPaddle();
                }
            }
        }

        private void FixedUpdate()
        {
            if (isOnPaddle) return;

            // Keep speed constant (only if moving)
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
            else if (lastFrameVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = lastFrameVelocity.normalized * currentSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.up * currentSpeed;
            }

            // Prevent horizontal-only trapping
            PreventHorizontalTrap();

            lastFrameVelocity = rb.linearVelocity;
        }

        public void SetOnPaddle(PaddleController paddle)
        {
            isOnPaddle = true;
            attachedPaddle = paddle;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Disable physics simulation while on paddle
            if (trailRenderer != null) trailRenderer.Clear();
        }

        private void LaunchFromPaddle()
        {
            isOnPaddle = false;
            rb.simulated = true;
            if (attachedPaddle != null)
            {
                attachedPaddle.RemoveCaughtBall(this);
            }

            // Launch slightly angled upwards
            Vector2 launchDir = new Vector2(Random.Range(-0.2f, 0.2f), 1f).normalized;
            Launch(launchDir * currentSpeed);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayLaunch();
        }

        public void Launch(Vector2 velocity)
        {
            isOnPaddle = false;
            rb.simulated = true;
            currentSpeed = velocity.magnitude;
            if (currentSpeed < baseSpeed) currentSpeed = baseSpeed;
            rb.linearVelocity = velocity.normalized * currentSpeed;
            lastFrameVelocity = rb.linearVelocity;
            if (trailRenderer != null) trailRenderer.Clear();
        }

        public void SlowDown()
        {
            currentSpeed = baseSpeed;
            if (!isOnPaddle)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }

        public bool IsPierceMode => isPierceMode;

        public void SetPierceMode(bool active)
        {
            isPierceMode = active;

            // Change visuals
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isPierceMode ? fireballColor : normalColor;
            }

            if (trailRenderer != null)
            {
                Gradient gradient = new Gradient();
                if (isPierceMode)
                {
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(fireballColor, 0.6f), new GradientColorKey(Color.red, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.4f, 0.6f), new GradientAlphaKey(0.0f, 1.0f) }
                    );
                }
                else
                {
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0f, 0.9f, 1f), 0.5f), new GradientColorKey(new Color(0.5f, 0f, 1f), 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
                    );
                }
                trailRenderer.colorGradient = gradient;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // If we hit the paddle, redirect the ball depending on where it landed
            PaddleController paddle = collision.gameObject.GetComponent<PaddleController>();
            if (paddle != null)
            {
                SoundManager.Instance.PlayBounce();
                paddle.TriggerBounceReaction();

                // If paddle is in Catch Mode, capture the ball
#if ENABLE_INPUT_SYSTEM
                bool isReleaseHeld = (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed) || 
                                     (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed);
#else
                bool isReleaseHeld = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
#endif
                if (isReleaseHeld)
                {
                    // If user is already holding down fire/release, do not catch, just bounce
                    BounceOffPaddle(collision, paddle);
                }
                else
                {
                    paddle.CatchBall(this);
                    if (isOnPaddle) return; // We were caught, exit collision logic
                    BounceOffPaddle(collision, paddle);
                }
                return;
            }

            // If we hit a brick
            Brick brick = collision.gameObject.GetComponent<Brick>();
            if (brick != null)
            {
                // In Pierce/Fireball mode, we don't bounce. We pass through!
                // Wait! If the brick is unbreakable (Steel), we might still want to bounce, or pierce if configured.
                // Usually unbreakable bricks should bounce the fireball, but breakable bricks are pierced.
                if (isPierceMode && !brick.IsUnbreakable)
                {
                    // Ignore collision physically so the ball can pass through
                    Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
                    StartCoroutine(RestoreCollisionRoutine(collision.collider, collision.otherCollider));
                    
                    brick.Hit(1); // Fireball deals damage

                    // Maintain incoming velocity so we pierce straight through without bouncing
                    if (lastFrameVelocity.sqrMagnitude > 0.01f)
                    {
                        rb.linearVelocity = lastFrameVelocity.normalized * currentSpeed;
                    }
                    lastFrameVelocity = rb.linearVelocity;
                    return; 
                }
                else
                {
                    brick.Hit(1);
                    SoundManager.Instance.PlayBounce();
                    // Reflected velocity is automatically calculated by Unity physics, 
                    // but we can reinforce speed and angle control here.
                    BounceOffWall(collision);
                }
                return;
            }

            // Normal walls or boundaries
            SoundManager.Instance.PlayBounce();
            BounceOffWall(collision);
        }

        private void BounceOffPaddle(Collision2D collision, PaddleController paddle)
        {
            // Position difference
            float relativeHitPos = transform.position.x - paddle.transform.position.x;
            
            // Normalize it between -1 and 1
            float paddleWidth = collision.collider.bounds.size.x;
            float normalizedHitPos = relativeHitPos / (paddleWidth / 2f);
            normalizedHitPos = Mathf.Clamp(normalizedHitPos, -1f, 1f);

            // Prevent perfectly vertical bounces by enforcing a minimum horizontal offset.
            // This avoids the ball getting trapped bouncing straight up and down,
            // especially when the paddle is at screen boundaries and cannot be moved further.
            float minOffset = 0.08f;
            if (Mathf.Abs(normalizedHitPos) < minOffset)
            {
                // Nudge it horizontally: preserve incoming direction if it exists,
                // otherwise push it towards the center of the screen.
                float nudgeSign = 0f;
                if (Mathf.Abs(lastFrameVelocity.x) > 0.01f)
                {
                    nudgeSign = Mathf.Sign(lastFrameVelocity.x);
                }
                else
                {
                    nudgeSign = paddle.transform.position.x > 0 ? -1f : 1f;
                }
                normalizedHitPos = minOffset * nudgeSign;
            }

            // Calculate new launch angle (e.g. max 60 degrees)
            float maxBounceAngle = 60f * Mathf.Deg2Rad;
            float angle = normalizedHitPos * maxBounceAngle;

            Vector2 newDir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;

            // Speed increment on paddle hit to build tension
            currentSpeed = Mathf.Min(currentSpeed + speedIncrement, maxSpeed);
            rb.linearVelocity = newDir * currentSpeed;
        }

        private void BounceOffWall(Collision2D collision)
        {
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.contacts[0].normal;
                Vector2 incomingVelocity = lastFrameVelocity.sqrMagnitude > 0.01f ? lastFrameVelocity : rb.linearVelocity;
                
                // Only reflect if moving towards the contact surface
                if (Vector2.Dot(incomingVelocity, normal) < 0f)
                {
                    Vector2 reflectDir = Vector2.Reflect(incomingVelocity, normal);
                    rb.linearVelocity = reflectDir.normalized * currentSpeed;
                    lastFrameVelocity = rb.linearVelocity;
                    return;
                }
            }

            // Fallback
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }

        private void PreventHorizontalTrap()
        {
            // If ball moves too flat, pull it back vertically
            Vector2 vel = rb.linearVelocity;
            float minVerticalSpeed = 2.0f;

            if (Mathf.Abs(vel.y) < minVerticalSpeed && Mathf.Abs(vel.x) > 0.1f)
            {
                float signY = vel.y >= 0 ? 1f : -1f;
                vel.y = signY * minVerticalSpeed;
                rb.linearVelocity = vel.normalized * currentSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we hit the Death Zone at the bottom (by name to avoid tag exception)
            if (other.name == "DeathZone")
            {
                GameManager.Instance.RemoveBall(this);
            }
        }

        private System.Collections.IEnumerator RestoreCollisionRoutine(Collider2D brickCollider, Collider2D ballCollider)
        {
            // Wait 0.25 seconds to allow the ball to completely pass through the brick
            yield return new WaitForSeconds(0.25f);

            // Restore collision if both still exist
            if (brickCollider != null && ballCollider != null)
            {
                Physics2D.IgnoreCollision(brickCollider, ballCollider, false);
            }
        }
    }
}
