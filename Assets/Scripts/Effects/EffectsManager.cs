using UnityEngine;
using System.Collections;

namespace Arkanoid.Effects
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        private Vector3 originalCameraPos;
        private Coroutine shakeCoroutine;

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

        private void Start()
        {
            if (Camera.main != null)
            {
                originalCameraPos = Camera.main.transform.position;
            }
            else
            {
                originalCameraPos = new Vector3(0, 0, -10f);
            }
        }

        public void ShakeCamera(float duration = 0.2f, float magnitude = 0.25f)
        {
            if (Camera.main == null) return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                Camera.main.transform.position = originalCameraPos;
            }

            shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                Camera.main.transform.position = new Vector3(
                    originalCameraPos.x + x,
                    originalCameraPos.y + y,
                    originalCameraPos.z
                );

                elapsed += Time.unscaledDeltaTime; // Shake works even when game is paused/slow-mo
                yield return null;
            }

            Camera.main.transform.position = originalCameraPos;
            shakeCoroutine = null;
        }

        public void SpawnFloatingText(string text, Vector3 position)
        {
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.position = position;

            FloatingText ft = textObj.AddComponent<FloatingText>();
            // Bright neon pink for scores
            ft.SetText(text, new Color(1f, 0f, 0.7f));
        }
    }
}
