#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Arkanoid.Core;
using Arkanoid.Gameplay;
using Arkanoid.UI;
using Arkanoid.Effects;
using System.IO;

namespace Arkanoid.Editor
{
    public class ArkanoidSceneSetup : EditorWindow
    {
        [MenuItem("Tools/Arkanoid/Setup Game Scene")]
        public static void SetupGame()
        {
            // 1. Create a new empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Ensure folder exists
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }
            
            string scenePath = "Assets/Scenes/Arkanoid.unity";

            // 2. Set up the Main Camera
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera");
                camObj.AddComponent<Camera>();
            }
            if (camObj.GetComponent<AudioListener>() == null)
            {
                camObj.AddComponent<AudioListener>();
            }
            Camera camera = camObj.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.02f, 0.07f, 1f); // Deep Space Dark Violet
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camObj.transform.position = new Vector3(0, 0, -10f);

            // Enable post processing on camera
            var additionalCameraData = camObj.GetComponent<UniversalAdditionalCameraData>();
            if (additionalCameraData == null)
            {
                additionalCameraData = camObj.AddComponent<UniversalAdditionalCameraData>();
            }
            additionalCameraData.renderPostProcessing = true;

            // 3. Create the play field borders (neon pink vector look)
            GameObject bordersObj = new GameObject("Borders");
            bordersObj.transform.position = Vector3.zero;

            Sprite squareSprite = GetOrCreateSquareSprite();
            Sprite circleSprite = GetOrCreateCircleSprite();

            Color neonPink = new Color(1f, 0f, 0.5f, 1f);

            // Left Wall
            GameObject leftWall = CreateBorderSegment("LeftWall", new Vector3(-6.1f, 0f, 0f), new Vector3(0.2f, 17f, 1f), neonPink, squareSprite, bordersObj.transform);
            // Right Wall
            GameObject rightWall = CreateBorderSegment("RightWall", new Vector3(6.1f, 0f, 0f), new Vector3(0.2f, 17f, 1f), neonPink, squareSprite, bordersObj.transform);
            // Top Wall
            GameObject topWall = CreateBorderSegment("TopWall", new Vector3(0f, 8.4f, 0f), new Vector3(12.4f, 0.2f, 1f), neonPink, squareSprite, bordersObj.transform);
            
            // Bottom DeathZone
            GameObject deathZone = new GameObject("DeathZone");
            deathZone.transform.position = new Vector3(0f, -8.6f, 0f);
            deathZone.transform.localScale = new Vector3(15f, 0.5f, 1f);
            var dzCol = deathZone.AddComponent<BoxCollider2D>();
            dzCol.isTrigger = true;

            // 4. Create Background Neon Grid
            GameObject bgObj = new GameObject("GridBackground");
            bgObj.AddComponent<GridBackground>();

            // 5. Create Prototypes Folder (Disabled templates in scene)
            GameObject prototypesObj = new GameObject("Prototypes");
            prototypesObj.SetActive(false);

            // --- Ball Prototype ---
            GameObject ballProto = new GameObject("BallPrototype");
            ballProto.transform.parent = prototypesObj.transform;
            ballProto.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            
            var ballSr = ballProto.AddComponent<SpriteRenderer>();
            ballSr.sprite = circleSprite;
            ballSr.color = Color.white;

            ballProto.AddComponent<CircleCollider2D>();
            var ballRb = ballProto.AddComponent<Rigidbody2D>();
            ballRb.gravityScale = 0f;
            ballRb.bodyType = RigidbodyType2D.Dynamic;
            ballRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            ballRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var ballTrail = ballProto.AddComponent<TrailRenderer>();
            // Materials can use the default Line material
            ballTrail.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

            var ballCtrl = ballProto.AddComponent<BallController>();
            ballCtrl.trailRenderer = ballTrail;

            // --- Brick Prototype ---
            GameObject brickProto = new GameObject("BrickPrototype");
            brickProto.transform.parent = prototypesObj.transform;
            brickProto.transform.localScale = new Vector3(0.9f, 0.35f, 1f);
            
            var brickSr = brickProto.AddComponent<SpriteRenderer>();
            brickSr.sprite = squareSprite;
            brickSr.color = Color.white;

            brickProto.AddComponent<BoxCollider2D>();
            var brickComp = brickProto.AddComponent<Brick>();

            // --- PowerUp Prototype ---
            GameObject puProto = new GameObject("PowerUpPrototype");
            puProto.transform.parent = prototypesObj.transform;
            puProto.transform.localScale = new Vector3(0.35f, 0.6f, 1f);

            var puSr = puProto.AddComponent<SpriteRenderer>();
            puSr.sprite = squareSprite; // Rounded rectangle fits capsule style
            puSr.color = Color.white;

            var puCol = puProto.AddComponent<CapsuleCollider2D>();
            puCol.isTrigger = true;
            var puComp = puProto.AddComponent<PowerUp>();
            var puRb = puProto.GetComponent<Rigidbody2D>();
            if (puRb != null)
            {
                puRb.bodyType = RigidbodyType2D.Kinematic;
                puRb.simulated = true;
                puRb.gravityScale = 0f;
            }
            brickComp.powerUpPrefab = puComp;

            // --- Laser Prototype ---
            GameObject laserProto = new GameObject("LaserPrototype");
            laserProto.transform.parent = prototypesObj.transform;
            laserProto.transform.localScale = new Vector3(0.12f, 0.5f, 1f);

            var laserSr = laserProto.AddComponent<SpriteRenderer>();
            laserSr.sprite = squareSprite;
            laserSr.color = Color.red;

            var laserCol = laserProto.AddComponent<CapsuleCollider2D>();
            laserCol.isTrigger = true;
            var laserComp = laserProto.AddComponent<LaserBeam>();
            var laserRb = laserProto.GetComponent<Rigidbody2D>();
            if (laserRb != null)
            {
                laserRb.bodyType = RigidbodyType2D.Kinematic;
                laserRb.simulated = true;
                laserRb.gravityScale = 0f;
            }

            // 6. Create Paddle GameObject
            GameObject paddleObj = new GameObject("Paddle");
            paddleObj.transform.position = new Vector3(0f, -6.5f, 0f);
            paddleObj.transform.localScale = new Vector3(1.5f, 0.3f, 1f);

            var paddleSr = paddleObj.AddComponent<SpriteRenderer>();
            paddleSr.sprite = squareSprite;
            paddleSr.color = new Color(0f, 0.9f, 1f); // Cyan default

            paddleObj.AddComponent<BoxCollider2D>();
            var paddleCtrl = paddleObj.AddComponent<PaddleController>();
            paddleCtrl.laserPrefab = laserComp;

            // 7. Create Managers GameObject
            GameObject managersObj = new GameObject("Managers");
            
            var soundMgr = managersObj.AddComponent<SoundManager>();
            var levelMgr = managersObj.AddComponent<LevelManager>();
            levelMgr.brickPrefab = brickComp;

            // Ensure Levels directory exists
            if (!Directory.Exists("Assets/Levels"))
            {
                Directory.CreateDirectory("Assets/Levels");
                AssetDatabase.Refresh();
            }

            // Create default levels as assets and link them to LevelManager
            var defaultLayouts = new string[][]
            {
                new string[]
                {
                    "............",
                    "....RRRR....",
                    "..OOOOOOOO..",
                    "GGGGGGGGGGGG",
                    "YYYYYYYYYYYY",
                    "BBBBBBBBBBBB"
                },
                new string[]
                {
                    "S..S....S..S",
                    "S.R.O..O.R.S",
                    "S.O.Y..Y.O.S",
                    "..Y.G..G.Y..",
                    "..G.B..B.G..",
                    "S..S....S..S"
                },
                new string[]
                {
                    "SSSSSSSSSSSS",
                    "SRRS.G.SBTBS",
                    "S.RSG.GSBTBS",
                    "S.RSGGGS.B.S",
                    "S.RSG.GS.B.S",
                    "SSSSSSSSSSSS"
                },
                new string[]
                {
                    "....R....R..",
                    ".....R..R...",
                    "....RRRRRR..",
                    "..RR.RR.RR.R",
                    "RRRRRRRRRRRR",
                    "R.R.RRRR.R.R",
                    "R...R..R...R",
                    "....R..R...."
                }
            };

            levelMgr.levels = new System.Collections.Generic.List<ArkanoidLevel>();
            for (int i = 0; i < defaultLayouts.Length; i++)
            {
                string path = $"Assets/Levels/Level_{i + 1}.asset";
                ArkanoidLevel levelAsset = AssetDatabase.LoadAssetAtPath<ArkanoidLevel>(path);
                if (levelAsset == null)
                {
                    levelAsset = ScriptableObject.CreateInstance<ArkanoidLevel>();
                    levelAsset.levelName = $"Level {i + 1}";
                    
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    string[] rows = defaultLayouts[i];
                    for (int r = 0; r < rows.Length; r++)
                    {
                        sb.Append(rows[r]);
                        if (r < rows.Length - 1) sb.Append("\n");
                    }
                    levelAsset.asciiLayout = sb.ToString();
                    levelAsset.rows = rows.Length;
                    levelAsset.columns = rows[0].Length;

                    AssetDatabase.CreateAsset(levelAsset, path);
                }
                levelMgr.levels.Add(levelAsset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var effectsMgr = managersObj.AddComponent<EffectsManager>();
            
            var gameMgr = managersObj.AddComponent<GameManager>();
            gameMgr.paddle = paddleCtrl;
            gameMgr.ballPrefab = ballCtrl;

            // 8. Create Post Processing Volume
            GameObject volumeObj = new GameObject("PostProcessingVolume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            
            // Add Bloom & Vignette programmatically
            try
            {
                Bloom bloom = profile.Add<Bloom>();
                bloom.active = true;
                bloom.intensity.Override(1.3f);
                bloom.threshold.Override(0.85f);
                bloom.scatter.Override(0.6f);

                Vignette vignette = profile.Add<Vignette>();
                vignette.active = true;
                vignette.intensity.Override(0.35f);
                vignette.smoothness.Override(0.4f);
                vignette.color.Override(new Color(0.02f, 0.01f, 0.05f));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Post-processing profile creation setup: " + e.Message);
            }
            volume.sharedProfile = profile;
            AssetDatabase.CreateAsset(profile, "Assets/Settings/ArkanoidPostProfile.asset");

            // 9. Setup UI Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Setup EventSystem
            GameObject eventSystemObj = GameObject.Find("EventSystem");
            if (eventSystemObj == null)
            {
                eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                #if ENABLE_INPUT_SYSTEM
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                #else
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                #endif
            }

            // HUD Controller Panel
            GameObject hudPanel = new GameObject("HUDPanel");
            hudPanel.transform.SetParent(canvasObj.transform, false);
            var hudRect = hudPanel.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(1f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0f, 0f);
            hudRect.sizeDelta = new Vector2(0f, 100f);

            var hudCtrl = hudPanel.AddComponent<HUDController>();
            gameMgr.hudController = hudCtrl;

            Color neonCyan = new Color(0f, 0.9f, 1f);
            Color neonOrange = new Color(1f, 0.6f, 0f);
            Color neonGreen = new Color(0f, 1f, 0.5f);

            hudCtrl.scoreText = CreateText(hudPanel, "ScoreText", "PUNTS: 000000", 30, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(400f, 60f), TextAnchor.MiddleLeft);
            hudCtrl.highScoreText = CreateText(hudPanel, "HighScoreText", "RÈCORD: 000000", 30, neonOrange, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(450f, 60f), TextAnchor.MiddleCenter);
            hudCtrl.levelText = CreateText(hudPanel, "LevelText", "NIVELL: 1", 30, neonCyan, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-400f, 0f), new Vector2(250f, 60f), TextAnchor.MiddleRight);
            hudCtrl.livesText = CreateText(hudPanel, "LivesText", "VIDES: ▲ ▲ ▲", 30, neonGreen, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50f, 0f), new Vector2(300f, 60f), TextAnchor.MiddleRight);
            hudCtrl.powerUpText = CreateText(canvasObj, "PowerUpText", "- EXPANSIÓ ACTIVA -", 32, neonPink, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(800f, 60f), TextAnchor.MiddleCenter);
            hudCtrl.powerUpText.gameObject.SetActive(false);

            // Menu Controller
            GameObject menusObj = new GameObject("MenuController");
            menusObj.transform.SetParent(canvasObj.transform, false);
            var menusRect = menusObj.AddComponent<RectTransform>();
            menusRect.anchorMin = Vector2.zero;
            menusRect.anchorMax = Vector2.one;
            menusRect.sizeDelta = Vector2.zero;

            var menuCtrl = menusObj.AddComponent<MenuController>();
            gameMgr.menuController = menuCtrl;

            // Helper to make overlay panel background
            Color overlayBg = new Color(0.03f, 0.01f, 0.06f, 0.9f);

            // --- Main Menu Panel ---
            GameObject mainMenu = CreateUIPanel("MainMenuPanel", canvasObj.transform, overlayBg);
            menuCtrl.mainMenuPanel = mainMenu;
            CreateText(mainMenu, "TitleText", "NEON FLUX\nARKANOID", 72, neonPink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 200f), TextAnchor.MiddleCenter);
            menuCtrl.mainMenuHighScoreText = CreateText(mainMenu, "HighScore", "RÈCORD: 000000", 28, neonOrange, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.playButton = CreateUIButton(mainMenu, "PlayButton", "INICIAR PARTIDA", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(280f, 65f), neonCyan);
            menuCtrl.mainMenuResolutionButton = CreateUIButton(mainMenu, "ResolutionButton", "RESOLUCIÓ: 1920x1080", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(340f, 65f), neonCyan);
            menuCtrl.exitButton = CreateUIButton(mainMenu, "ExitButton", "SORTIR DEL SISTEMA", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(280f, 65f), neonCyan);

            // --- Pause Panel ---
            GameObject pausePanel = CreateUIPanel("PausePanel", canvasObj.transform, new Color(0.03f, 0.01f, 0.06f, 0.8f));
            menuCtrl.pausePanel = pausePanel;
            CreateText(pausePanel, "TitleText", "SISTEMA EN PAUSA", 60, neonOrange, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(800f, 100f), TextAnchor.MiddleCenter);
            menuCtrl.resumeButton = CreateUIButton(pausePanel, "ResumeButton", "REPRENDRE", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(280f, 65f), neonCyan);
            menuCtrl.pauseResolutionButton = CreateUIButton(pausePanel, "ResolutionButton", "RESOLUCIÓ: 1920x1080", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(340f, 65f), neonCyan);
            var pauseMenuBtn = CreateUIButton(pausePanel, "MenuButton", "MENÚ PRINCIPAL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(280f, 65f), neonCyan);

            // --- Game Over Panel ---
            GameObject gameOver = CreateUIPanel("GameOverPanel", canvasObj.transform, overlayBg);
            menuCtrl.gameOverPanel = gameOver;
            CreateText(gameOver, "TitleText", "FI DE PARTIDA", 72, Color.red, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 120f), TextAnchor.MiddleCenter);
            menuCtrl.gameOverScoreText = CreateText(gameOver, "ScoreText", "PUNTUACIÓ FINAL: 000000", 30, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.gameOverHighScoreText = CreateText(gameOver, "HighScoreText", "RÈCORD: 000000", 24, neonOrange, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.retryButton = CreateUIButton(gameOver, "RetryButton", "REINICIAR MATRIU", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(280f, 65f), neonCyan);
            var goMenuBtn = CreateUIButton(gameOver, "MenuButton", "MENÚ PRINCIPAL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(280f, 65f), neonCyan);

            // --- Level Complete Panel ---
            GameObject levelComplete = CreateUIPanel("LevelCompletePanel", canvasObj.transform, overlayBg);
            menuCtrl.levelCompletePanel = levelComplete;
            menuCtrl.levelCompleteTitleText = CreateText(levelComplete, "TitleText", "NIVELL 1 SUPERAT", 60, neonGreen, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(800f, 100f), TextAnchor.MiddleCenter);
            menuCtrl.nextLevelButton = CreateUIButton(levelComplete, "NextLevelButton", "SEGÜENT FASE", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(280f, 65f), neonCyan);

            // --- Victory Panel ---
            GameObject victory = CreateUIPanel("VictoryPanel", canvasObj.transform, overlayBg);
            menuCtrl.victoryPanel = victory;
            CreateText(victory, "TitleText", "VICTÒRIA ACONSEGUIDA", 68, neonGreen, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(800f, 120f), TextAnchor.MiddleCenter);
            CreateText(victory, "SubtitleText", "SISTEMA DEPURAT COMPLETAMENT", 22, neonCyan, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(800f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.victoryScoreText = CreateText(victory, "ScoreText", "PUNTUACIÓ FINAL: 000000", 30, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.victoryHighScoreText = CreateText(victory, "HighScoreText", "RÈCORD: 000000", 24, neonOrange, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
            menuCtrl.victoryPlayAgainButton = CreateUIButton(victory, "PlayAgainButton", "REINICIAR SISTEMA", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(280f, 65f), neonCyan);
            var vicMenuBtn = CreateUIButton(victory, "MenuButton", "MENÚ PRINCIPAL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(280f, 65f), neonCyan);

            // Connect return menu buttons
            menuCtrl.mainMenuButton = pauseMenuBtn; // Just bind one to satisfy field. Actually we can bind all menu buttons to the callback!
            pauseMenuBtn.onClick.AddListener(OnReturnMenuClicked);
            goMenuBtn.onClick.AddListener(OnReturnMenuClicked);
            vicMenuBtn.onClick.AddListener(OnReturnMenuClicked);

            // 10. Save the scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Arkanoid Scene built and saved successfully to: {scenePath}");

            // 11. Add Scene to Build Settings if not already there
            AddSceneToBuildSettings(scenePath);
        }

        private static void OnReturnMenuClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.ShowMainMenu();
        }

        private static GameObject CreateBorderSegment(string name, Vector3 pos, Vector3 scale, Color color, Sprite sprite, Transform parent)
        {
            GameObject seg = new GameObject(name);
            seg.transform.position = pos;
            seg.transform.localScale = scale;
            seg.transform.parent = parent;

            var sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;

            seg.AddComponent<BoxCollider2D>();
            return seg;
        }

        private static GameObject CreateUIPanel(string name, Transform parent, Color bgColor)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor;

            return panel;
        }

        private static Text CreateText(GameObject parent, string name, string text, int fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, TextAnchor align = TextAnchor.MiddleCenter)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
            {
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts.Length > 0) t.font = fonts[0];
            }
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;

            // Add simple neon drop shadow
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(color.r, color.g, color.b, 0.4f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return t;
        }

        private static Button CreateUIButton(GameObject parent, string name, string labelText, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color outlineColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var img = go.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.06f, 0.04f, 0.12f, 0.95f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // Outline for neon buttons
            var outline = go.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Hover / Pressed colors
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0f, 0.5f, 1f); // Neon pink highlight
            cb.pressedColor = new Color(0f, 0.9f, 1f, 1f); // Neon cyan pressed
            cb.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.colors = cb;

            // Label
            CreateText(go, "Text", labelText, 20, outlineColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            return btn;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            
            // Check if already in build settings
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            // Add it
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            System.Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }

        private static Sprite GetOrCreateSquareSprite()
        {
            if (!Directory.Exists("Assets/Settings"))
            {
                Directory.CreateDirectory("Assets/Settings");
            }
            string path = "Assets/Settings/ArkanoidSquare.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32; // exactly 1x1 unit in world space
                    importer.SaveAndReimport();
                }
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return sprite;
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            if (!Directory.Exists("Assets/Settings"))
            {
                Directory.CreateDirectory("Assets/Settings");
            }
            string path = "Assets/Settings/ArkanoidCircle.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color[] colors = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        float dx = x - 31.5f;
                        float dy = y - 31.5f;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= 30.5f)
                        {
                            colors[y * 64 + x] = Color.white;
                        }
                        else if (dist <= 31.5f)
                        {
                            float alpha = 31.5f - dist;
                            colors[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
                        }
                        else
                        {
                            colors[y * 64 + x] = Color.clear;
                        }
                    }
                }
                tex.SetPixels(colors);
                tex.Apply();
                
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 64; // exactly 1x1 unit in world space
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return sprite;
        }
    }
}
#endif
