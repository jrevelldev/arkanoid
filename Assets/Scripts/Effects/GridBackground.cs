using UnityEngine;

namespace Arkanoid.Effects
{
    public class GridBackground : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float spacing = 1.0f;
        public float width = 12f;
        public float height = 16f;
        public float scrollSpeed = 0.6f;
        
        [Header("Visuals")]
        public Color gridColor = new Color(0.2f, 0.05f, 0.45f, 0.35f); // Neon purple / indigo

        private Material lineMaterial;
        private float scrollOffset = 0f;

        private void Start()
        {
            CreateLineMaterial();
        }

        private void CreateLineMaterial()
        {
            if (lineMaterial == null)
            {
                // Internal colored shader is standard and works everywhere, supports vertex colors and alpha
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                if (shader != null)
                {
                    lineMaterial = new Material(shader);
                }
            }
        }

        private void Update()
        {
            // Scroll the horizontal lines downward to simulate speed
            scrollOffset = Mathf.Repeat(scrollOffset - scrollSpeed * Time.deltaTime, spacing);
        }

        private void OnRenderObject()
        {
            if (lineMaterial == null) CreateLineMaterial();
            if (lineMaterial == null) return;

            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Align with world coordinates
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            GL.Color(gridColor);

            float halfW = width / 2f;
            float halfH = height / 2f;

            // Draw vertical grid lines (static)
            for (float x = -halfW; x <= halfW + 0.01f; x += spacing)
            {
                GL.Vertex3(x, -halfH, 0);
                GL.Vertex3(x, halfH, 0);
            }

            // Draw scrolling horizontal grid lines
            for (float y = -halfH - spacing; y <= halfH + spacing; y += spacing)
            {
                float drawY = y + scrollOffset;
                // Clamp within bounds to prevent drawing outside boundary limits
                if (drawY >= -halfH && drawY <= halfH)
                {
                    GL.Vertex3(-halfW, drawY, 0);
                    GL.Vertex3(halfW, drawY, 0);
                }
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}
