using UnityEngine;
using Arkanoid.Core;
using System.Collections.Generic;

namespace Arkanoid.Gameplay
{
    public class PaddleController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float keyboardSpeed = 12f;
        public float boundaryLimit = 6.0f;

        [Header("Laser Settings")]
        public LaserBeam laserPrefab;
        public float laserCooldown = 0.4f;

        [Header("Aesthetic Settings")]
        public Color normalColor = new Color(0f, 0.9f, 1f); // Neon cyan
        public Color expandColor = new Color(1f, 0.08f, 0.58f); // Neon pink
        public Color laserColor = new Color(1f, 0.1f, 0.1f); // Neon red
        public Color catchColor = new Color(0f, 1f, 0.5f); // Neon green

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;

        // Power-up States
        private bool isExpandMode = false;
        private bool isLaserMode = false;
        private bool isCatchMode = false;

        // Targeting scale and size
        private Vector3 baseScale = new Vector3(1.5f, 0.3f, 1f);
        private Vector3 currentTargetScale;
        private Vector3 visualScale;

        // Laser control
        private float lastLaserTime = 0f;

        // Input and control
        private float lastMouseX;
        private bool useMouseControl = false;

        // Active caught balls
        private List<BallController> caughtBalls = new List<BallController>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            // Setup default size if not configured
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = baseScale;
            }
            currentTargetScale = transform.localScale;
            visualScale = currentTargetScale;
            
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                lastMouseX = UnityEngine.InputSystem.Mouse.current.position.ReadValue().x;
            }
#else
            lastMouseX = Input.mousePosition.x;
#endif
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleMovement();
            HandleActions();
            UpdateVisuals();
        }

        private void HandleMovement()
        {
            float targetX = transform.position.x;
            
            // 1. Check Keyboard/Gamepad Input
            float moveInput = 0f;
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput = -1f;
                else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput = 1f;
            }

            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            if (gamepad != null)
            {
                float stickInput = gamepad.leftStick.x.ReadValue();
                float dpadInput = 0f;
                if (gamepad.dpad.left.isPressed) dpadInput = -1f;
                else if (gamepad.dpad.right.isPressed) dpadInput = 1f;

                float gamepadMove = Mathf.Abs(stickInput) > Mathf.Abs(dpadInput) ? stickInput : dpadInput;
                if (Mathf.Abs(gamepadMove) > 0.05f) // Deadzone check
                {
                    moveInput = gamepadMove;
                }
            }
#else
            moveInput = Input.GetAxisRaw("Horizontal");
#endif
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                targetX += moveInput * keyboardSpeed * Time.deltaTime;
                useMouseControl = false;
            }
            
            // 2. Check Mouse Input (if mouse moves horizontally)
            float currentMouseX = lastMouseX;
            Vector3 mousePosVal = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                Vector2 mouseVal2D = mouse.position.ReadValue();
                mousePosVal = new Vector3(mouseVal2D.x, mouseVal2D.y, 0f);
                currentMouseX = mousePosVal.x;
            }
#else
            mousePosVal = Input.mousePosition;
            currentMouseX = mousePosVal.x;
#endif
            if (Mathf.Abs(currentMouseX - lastMouseX) > 0.5f)
            {
                useMouseControl = true;
                lastMouseX = currentMouseX;
            }

            if (useMouseControl && Camera.main != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosVal);
                targetX = mouseWorldPos.x;
            }

            // Constrain movement based on current paddle size
            float halfWidth = (spriteRenderer != null ? spriteRenderer.bounds.size.x : transform.localScale.x) / 2f;
            float minX = -boundaryLimit + halfWidth;
            float maxX = boundaryLimit - halfWidth;
            targetX = Mathf.Clamp(targetX, minX, maxX);

            // Apply position
            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }

        private void HandleActions()
        {
            // Fire lasers or release balls
#if ENABLE_INPUT_SYSTEM
            bool actionTriggered = (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) || 
                                   (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) ||
                                   (UnityEngine.InputSystem.Gamepad.current != null && (UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame || UnityEngine.InputSystem.Gamepad.current.buttonWest.wasPressedThisFrame));
#else
            bool actionTriggered = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton2);
#endif
            if (actionTriggered)
            {
                // Release caught balls
                if (caughtBalls.Count > 0)
                {
                    ReleaseCaughtBalls();
                }
                
                // Fire lasers
                if (isLaserMode && Time.time >= lastLaserTime + laserCooldown)
                {
                    FireLasers();
                }
            }
        }

        private void UpdateVisuals()
        {
            // Set base target scale based on Expand Mode
            float targetWidth = isExpandMode ? baseScale.x * 1.6f : baseScale.x;
            currentTargetScale = new Vector3(targetWidth, baseScale.y, baseScale.z);

            // Apply spring/lerp back to target scale (squash/stretch logic)
            visualScale = Vector3.Lerp(visualScale, currentTargetScale, 15f * Time.deltaTime);
            transform.localScale = visualScale;

            // Update color based on current powerup state
            if (spriteRenderer != null)
            {
                Color targetColor = normalColor;
                if (isLaserMode) targetColor = laserColor;
                else if (isCatchMode) targetColor = catchColor;
                else if (isExpandMode) targetColor = expandColor;

                spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, 10f * Time.deltaTime);
            }
        }

        public void ResetPaddle()
        {
            isExpandMode = false;
            isLaserMode = false;
            isCatchMode = false;
            caughtBalls.Clear();
            visualScale = baseScale;
            transform.localScale = baseScale;
            if (spriteRenderer != null) spriteRenderer.color = normalColor;
        }

        public void SetExpandMode(bool active)
        {
            isExpandMode = active;
            TriggerWobble();
        }

        public void SetLaserMode(bool active)
        {
            isLaserMode = active;
            TriggerWobble();
        }

        public void SetCatchMode(bool active)
        {
            isCatchMode = active;
            TriggerWobble();
        }

        public void CatchBall(BallController ball)
        {
            if (isCatchMode && !caughtBalls.Contains(ball))
            {
                ball.SetOnPaddle(this);
                caughtBalls.Add(ball);
                if (SoundManager.Instance != null) SoundManager.Instance.PlayCatch();
            }
        }

        public void RemoveCaughtBall(BallController ball)
        {
            caughtBalls.Remove(ball);
        }

        private void ReleaseCaughtBalls()
        {
            // Capture a copy to avoid modification during loop
            List<BallController> temp = new List<BallController>(caughtBalls);
            caughtBalls.Clear();

            if (temp.Count > 0 && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLaunch();
            }

            foreach (var ball in temp)
            {
                if (ball != null)
                {
                    // Launch ball upwards with a slight angle based on position on paddle
                    float offset = ball.transform.position.x - transform.position.x;
                    float normalizedOffset = offset / (transform.localScale.x / 2f);
                    Vector2 dir = new Vector2(normalizedOffset * 0.4f, 1f).normalized;
                    ball.Launch(dir * ball.baseSpeed);
                }
            }
        }

        private void FireLasers()
        {
            lastLaserTime = Time.time;
            SoundManager.Instance.PlayLaser();

            float halfWidth = transform.localScale.x / 2f;
            Vector3 leftSpawn = transform.position + new Vector3(-halfWidth + 0.15f, 0.3f, 0f);
            Vector3 rightSpawn = transform.position + new Vector3(halfWidth - 0.15f, 0.3f, 0f);

            SpawnLaser(leftSpawn);
            SpawnLaser(rightSpawn);

            // Add firing kick visual feedback (squash Y slightly)
            visualScale = new Vector3(currentTargetScale.x, currentTargetScale.y * 0.6f, currentTargetScale.z);
        }

        private void SpawnLaser(Vector3 position)
        {
            if (laserPrefab != null)
            {
                Instantiate(laserPrefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback: Create laser programmatically
                GameObject laserObj = new GameObject("ProceduralLaser");
                laserObj.transform.position = position;
                
                SpriteRenderer sr = laserObj.AddComponent<SpriteRenderer>();
                Texture2D tex = Texture2D.whiteTexture;
                sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f), tex.width);
                sr.color = Color.red;

                laserObj.transform.localScale = new Vector3(0.12f, 0.5f, 1f);

                CapsuleCollider2D col = laserObj.AddComponent<CapsuleCollider2D>();
                col.isTrigger = true;

                laserObj.AddComponent<LaserBeam>();
            }
        }

        public void TriggerBounceReaction()
        {
            // Squash paddle vertically (Y goes down) and stretch horizontally (X goes up)
            visualScale = new Vector3(currentTargetScale.x * 1.25f, currentTargetScale.y * 0.5f, currentTargetScale.z);
        }

        public void TriggerWobble()
        {
            // Wobble scale to emphasize change
            visualScale = new Vector3(currentTargetScale.x * 0.8f, currentTargetScale.y * 1.5f, currentTargetScale.z);
        }
    }
}
