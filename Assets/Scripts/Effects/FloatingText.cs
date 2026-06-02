using UnityEngine;

namespace Arkanoid.Effects
{
    public class FloatingText : MonoBehaviour
    {
        public float speed = 1.8f;
        public float lifeTime = 0.65f;
        
        private TextMesh textMesh;
        private float elapsed = 0f;

        public void SetText(string text, Color color)
        {
            textMesh = GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMesh>();
            }

            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;
            
            // Render text in front of 2D sprites
            textMesh.GetComponent<MeshRenderer>().sortingOrder = 50;
        }

        private void Update()
        {
            // Scroll upward
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

            elapsed += Time.deltaTime;
            if (elapsed >= lifeTime)
            {
                Destroy(gameObject);
            }
            else if (textMesh != null)
            {
                // Fade out
                Color c = textMesh.color;
                c.a = Mathf.Lerp(1.0f, 0.0f, elapsed / lifeTime);
                textMesh.color = c;
            }
        }
    }
}
