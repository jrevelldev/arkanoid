using UnityEngine;
using System.Collections.Generic;
using Arkanoid.Gameplay;

namespace Arkanoid.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Settings")]
        public float brickWidth = 0.9f;
        public float brickHeight = 0.35f;
        public float spacingX = 0.1f;
        public float spacingY = 0.1f;
        public float startY = 4.0f; // Height to start spawning bricks

        [Header("References")]
        public Brick brickPrefab;

        private List<Brick> spawnedBricks = new List<Brick>();

        [Header("Levels")]
        public List<ArkanoidLevel> levels = new List<ArkanoidLevel>();

        // 4 default levels defined as ASCII grids as fallback.
        // Columns = 12, Rows = 8.
        private readonly string[][] defaultLevels = new string[][]
        {
            // Level 1: Simple rows
            new string[]
            {
                "............",
                "....RRRR....",
                "..OOOOOOOO..",
                "GGGGGGGGGGGG",
                "YYYYYYYYYYYY",
                "BBBBBBBBBBBB"
            },
            // Level 2: Alternating pillars and unbreakable blocks
            new string[]
            {
                "S..S....S..S",
                "S.R.O..O.R.S",
                "S.O.Y..Y.O.S",
                "..Y.G..G.Y..",
                "..G.B..B.G..",
                "S..S....S..S"
            },
            // Level 3: TAV layout with steel dividers
            new string[]
            {
                "SSSSSSSSSSSS",
                "SRRS.G.SBTBS",
                "S.RSG.GSBTBS",
                "S.RSGGGS.B.S",
                "S.RSG.GS.B.S",
                "SSSSSSSSSSSS"
            },
            // Level 4: Space Invader pattern
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

        public int TotalLevels => (levels != null && levels.Count > 0) ? levels.Count : defaultLevels.Length;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SpawnLevel(int levelIndex)
        {
            ClearBricks();

            int total = TotalLevels;
            if (levelIndex < 0 || levelIndex >= total)
            {
                Debug.LogError($"Level index {levelIndex} out of bounds!");
                return;
            }

            string[] layout;
            if (levels != null && levels.Count > 0)
            {
                layout = levels[levelIndex].GetLayoutLines();
            }
            else
            {
                layout = defaultLevels[levelIndex];
            }

            int rows = layout.Length;
            int cols = rows > 0 ? layout[0].Length : 0;

            if (cols == 0) return;

            // Compute total grid width to center bricks horizontally
            float totalWidth = cols * brickWidth + (cols - 1) * spacingX;
            float startX = -totalWidth / 2f + brickWidth / 2f;

            for (int r = 0; r < rows; r++)
            {
                string rowText = layout[r];
                int colCount = Mathf.Min(cols, rowText.Length);
                for (int c = 0; c < colCount; c++)
                {
                    char brickChar = rowText[c];
                    if (brickChar == '.') continue;

                    Vector3 spawnPos = new Vector3(
                        startX + c * (brickWidth + spacingX),
                        startY - r * (brickHeight + spacingY),
                        0f
                    );

                    SpawnBrick(brickChar, spawnPos);
                }
            }
        }

        private void SpawnBrick(char type, Vector3 position)
        {
            Brick brick;
            if (brickPrefab != null)
            {
                brick = Instantiate(brickPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                // Fallback: Create brick programmatically
                GameObject brickObj = new GameObject("ProceduralBrick");
                brickObj.transform.position = position;
                brickObj.transform.parent = transform;
                
                // Add SpriteRenderer
                SpriteRenderer sr = brickObj.AddComponent<SpriteRenderer>();
                // Create a white 1x1 sprite
                Texture2D tex = Texture2D.whiteTexture;
                sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f), tex.width);
                
                // Size it
                brickObj.transform.localScale = new Vector3(brickWidth, brickHeight, 1f);

                // Add BoxCollider2D
                brickObj.AddComponent<BoxCollider2D>();

                // Add Brick component
                brick = brickObj.AddComponent<Brick>();
            }

            // Initialize brick properties
            ConfigureBrick(brick, type);
            spawnedBricks.Add(brick);
        }

        private void ConfigureBrick(Brick brick, char type)
        {
            int maxHits = 1;
            int points = 50;
            Color baseColor = Color.white;
            bool unbreakable = false;

            switch (type)
            {
                case 'Y': // Yellow
                    maxHits = 1;
                    points = 50;
                    baseColor = new Color(1f, 0.92f, 0.016f); // Bright yellow
                    break;
                case 'G': // Green
                    maxHits = 1;
                    points = 60;
                    baseColor = new Color(0f, 1f, 0.2f); // Neon green
                    break;
                case 'B': // Blue
                    maxHits = 1;
                    points = 70;
                    baseColor = new Color(0f, 0.7f, 1f); // Neon blue
                    break;
                case 'O': // Orange
                    maxHits = 2;
                    points = 100;
                    baseColor = new Color(1f, 0.5f, 0f); // Bright orange
                    break;
                case 'I': // Indigo
                    maxHits = 2;
                    points = 120;
                    baseColor = new Color(0.5f, 0f, 1f); // Neon purple
                    break;
                case 'R': // Red
                    maxHits = 3;
                    points = 150;
                    baseColor = new Color(1f, 0f, 0.4f); // Neon magenta/red
                    break;
                case 'S': // Steel
                    maxHits = 9999;
                    points = 0;
                    baseColor = new Color(0.5f, 0.5f, 0.6f); // Metallic grey
                    unbreakable = true;
                    break;
            }

            brick.Initialize(maxHits, points, baseColor, unbreakable);
        }

        public int GetBreakableBricksCount()
        {
            int count = 0;
            foreach (var b in spawnedBricks)
            {
                if (b != null && !b.IsUnbreakable) count++;
            }
            return count;
        }

        public void RemoveBrick(Brick brick)
        {
            if (spawnedBricks.Contains(brick))
            {
                spawnedBricks.Remove(brick);
            }
        }

        public void ClearBricks()
        {
            foreach (var b in spawnedBricks)
            {
                if (b != null) Destroy(b.gameObject);
            }
            spawnedBricks.Clear();
        }
    }
}
