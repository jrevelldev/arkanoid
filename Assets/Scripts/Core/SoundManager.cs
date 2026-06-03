using UnityEngine;

namespace Arkanoid.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource audioSource;
        
        private AudioClip bounceClip;
        private AudioClip brickHitClip;
        private AudioClip brickBreakClip;
        private AudioClip powerUpClip;
        private AudioClip laserClip;
        private AudioClip deathClip;
        private AudioClip winClip;
        private AudioClip lossClip;
        private AudioClip launchClip;
        private AudioClip catchClip;
        private AudioClip powerUpDropClip;
        private AudioClip uiSelectClip;
        private AudioClip uiConfirmClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                audioSource = gameObject.AddComponent<AudioSource>();
                // Configure audio source settings for retro crispness
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.0f; // 2D sound
                GenerateAudioClips();
                EnsureAudioListener();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void EnsureAudioListener()
        {
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    mainCam.gameObject.AddComponent<AudioListener>();
                    Debug.Log("SoundManager: Added AudioListener to Main Camera at runtime.");
                }
                else
                {
                    gameObject.AddComponent<AudioListener>();
                    Debug.Log("SoundManager: No Main Camera found, added AudioListener to SoundManager at runtime.");
                }
            }
        }

        private void GenerateAudioClips()
        {
            bounceClip = CreateTone(0.08f, (t, d) => {
                float freq = Mathf.Lerp(450f, 650f, t / d);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d);
            });

            brickHitClip = CreateTone(0.08f, (t, d) => {
                float freq = Mathf.Lerp(350f, 200f, t / d);
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)); // Square wave
                return wave * (1f - t / d) * 0.2f;
            });

            brickBreakClip = CreateTone(0.18f, (t, d) => {
                float freq = Mathf.Lerp(280f, 60f, t / d);
                float noise = Random.Range(-1f, 1f) * 0.25f;
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.4f;
                return (wave + noise) * (1f - t / d) * 0.35f;
            });

            powerUpClip = CreateTone(0.35f, (t, d) => {
                float phase = t / d;
                float freq = 440f; // A4
                if (phase > 0.66f) freq = 880f; // A5
                else if (phase > 0.33f) freq = 659.25f; // E5
                
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d);
            });

            laserClip = CreateTone(0.09f, (t, d) => {
                float freq = Mathf.Lerp(1400f, 250f, t / d);
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                return wave * (1f - t / d) * 0.2f;
            });

            deathClip = CreateTone(0.5f, (t, d) => {
                float freq = Mathf.Lerp(400f, 40f, t / d);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                return wave * (1f - t / d) * 0.6f;
            });

            winClip = CreateTone(0.9f, (t, d) => {
                float phase = t / d;
                float freq = 261.63f; // C4
                if (phase > 0.75f) freq = 523.25f; // C5
                else if (phase > 0.5f) freq = 392.00f; // G4
                else if (phase > 0.25f) freq = 329.63f; // E4
                
                // Arpeggiated sine wave
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d) * 0.5f;
            });

            lossClip = CreateTone(1.2f, (t, d) => {
                float freq = Mathf.Lerp(180f, 40f, t / d);
                float noise = Random.Range(-1f, 1f) * 0.15f;
                return (Mathf.Sin(2f * Mathf.PI * freq * t) + noise) * (1f - t / d) * 0.5f;
            });

            launchClip = CreateTone(0.15f, (t, d) => {
                float freq = Mathf.Lerp(400f, 900f, t / d);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d) * 0.4f;
            });

            catchClip = CreateTone(0.12f, (t, d) => {
                float freq = Mathf.Lerp(600f, 250f, t / d);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d) * 0.4f;
            });

            powerUpDropClip = CreateTone(0.25f, (t, d) => {
                float phase = t / d;
                float freq = 300f;
                if (phase > 0.66f) freq = 500f;
                else if (phase > 0.33f) freq = 400f;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d) * 0.3f;
            });

            uiSelectClip = CreateTone(0.04f, (t, d) => {
                float freq = 800f;
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                return wave * (1f - t / d) * 0.15f;
            });

            uiConfirmClip = CreateTone(0.12f, (t, d) => {
                float phase = t / d;
                float freq = phase > 0.5f ? 1200f : 1000f;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / d) * 0.3f;
            });
        }

        private AudioClip CreateTone(float duration, System.Func<float, float, float> waveFunc)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                samples[i] = waveFunc(time, duration);
            }

            AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void PlayBounce() => audioSource.PlayOneShot(bounceClip);
        public void PlayBrickHit() => audioSource.PlayOneShot(brickHitClip);
        public void PlayBrickBreak() => audioSource.PlayOneShot(brickBreakClip);
        public void PlayPowerUp() => audioSource.PlayOneShot(powerUpClip);
        public void PlayLaser() => audioSource.PlayOneShot(laserClip);
        public void PlayDeath() => audioSource.PlayOneShot(deathClip);
        public void PlayWin() => audioSource.PlayOneShot(winClip);
        public void PlayLoss() => audioSource.PlayOneShot(lossClip);
        public void PlayLaunch() => audioSource.PlayOneShot(launchClip);
        public void PlayCatch() => audioSource.PlayOneShot(catchClip);
        public void PlayPowerUpDrop() => audioSource.PlayOneShot(powerUpDropClip);
        public void PlayUISelect() => audioSource.PlayOneShot(uiSelectClip);
        public void PlayUIConfirm() => audioSource.PlayOneShot(uiConfirmClip);
    }
}
