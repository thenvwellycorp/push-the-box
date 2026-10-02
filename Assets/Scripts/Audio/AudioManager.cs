using System;
using UnityEngine;
using PushTheBox.Save;

namespace PushTheBox.Audio
{
    public enum SoundType
    {
        PlayerMove,
        BoxPush,
        BoxOnTarget,
        Undo,
        ButtonClick,
        LevelComplete,
        DeadlockWarning,
        CoinReward,
        GameOver
    }

    /// <summary>
    /// Centralized Audio manager. Decoupled from gameplay and UI classes.
    /// Provides built-in procedural synthesis fallback if custom audio clips are not assigned.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips (Optional - procedural fallback generated if empty)")]
        [SerializeField] private AudioClip moveClip;
        [SerializeField] private AudioClip pushClip;
        [SerializeField] private AudioClip targetClip;
        [SerializeField] private AudioClip undoClip;
        [SerializeField] private AudioClip buttonClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip warningClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip gameOverClip;

        private AudioSource sfxSource;
        private bool isMuted = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            GenerateProceduralAudioFallbacks();
        }

        private void Start()
        {
            if (SaveManager.Instance != null)
            {
                isMuted = !SaveManager.Instance.SoundEnabled;
            }
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetSoundEnabled(!muted);
            }
        }

        public bool IsMuted => isMuted;

        public void PlaySound(SoundType type)
        {
            if (isMuted || sfxSource == null) return;

            AudioClip clip = GetClipForType(type);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private AudioClip GetClipForType(SoundType type)
        {
            switch (type)
            {
                case SoundType.PlayerMove: return moveClip;
                case SoundType.BoxPush: return pushClip;
                case SoundType.BoxOnTarget: return targetClip;
                case SoundType.Undo: return undoClip;
                case SoundType.ButtonClick: return buttonClip;
                case SoundType.LevelComplete: return winClip;
                case SoundType.DeadlockWarning: return warningClip;
                case SoundType.CoinReward: return coinClip;
                case SoundType.GameOver: return gameOverClip != null ? gameOverClip : warningClip;
                default: return null;
            }
        }

        /// <summary>
        /// Generates pleasant synthetic sound effects programmatically so the game is
        /// completely functional without requiring external audio file imports.
        /// </summary>
        private void GenerateProceduralAudioFallbacks()
        {
            if (moveClip == null)
                moveClip = SynthesizeTone("SFX_Move", 300f, 0.06f, 0.25f, WaveType.Sine);

            if (pushClip == null)
                pushClip = SynthesizeTone("SFX_Push", 180f, 0.12f, 0.4f, WaveType.Square);

            if (targetClip == null)
                targetClip = SynthesizeChime("SFX_Target", new float[] { 523.25f, 659.25f, 783.99f }, 0.25f);

            if (undoClip == null)
                undoClip = SynthesizeTone("SFX_Undo", 240f, 0.08f, 0.3f, WaveType.Sine, true);

            if (buttonClip == null)
                buttonClip = SynthesizeTone("SFX_Click", 600f, 0.04f, 0.2f, WaveType.Sine);

            if (winClip == null)
                winClip = SynthesizeFanfare("SFX_Win");

            if (warningClip == null)
                warningClip = SynthesizeTone("SFX_Warning", 220f, 0.18f, 0.35f, WaveType.Sawtooth);

            if (coinClip == null)
                coinClip = SynthesizeChime("SFX_Coin", new float[] { 987.77f, 1318.51f }, 0.28f);

            if (gameOverClip == null)
                gameOverClip = SynthesizeTone("SFX_GameOver", 140f, 0.4f, 0.4f, WaveType.Sawtooth, true);
        }

        private enum WaveType { Sine, Square, Sawtooth }

        private AudioClip SynthesizeTone(string name, float frequency, float duration, float volume, WaveType wave = WaveType.Sine, bool pitchDown = false)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = pitchDown ? Mathf.Lerp(frequency * 1.5f, frequency * 0.7f, t) : frequency;
                float phase = 2f * Mathf.PI * currentFreq * ((float)i / sampleRate);

                float val = 0f;
                switch (wave)
                {
                    case WaveType.Sine:
                        val = Mathf.Sin(phase);
                        break;
                    case WaveType.Square:
                        val = Mathf.Sin(phase) >= 0 ? 0.6f : -0.6f;
                        break;
                    case WaveType.Sawtooth:
                        val = (2f * ((phase / (2f * Mathf.PI)) % 1f)) - 1f;
                        break;
                }

                // Envelope decay to prevent clicks
                float envelope = 1f - t;
                samples[i] = val * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeChime(string name, float[] notes, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Exp(-t * 5f);
                float val = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float phase = 2f * Mathf.PI * notes[n] * ((float)i / sampleRate);
                    val += Mathf.Sin(phase) * (0.3f / notes.Length);
                }

                samples[i] = val * envelope;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip SynthesizeFanfare(string name)
        {
            float duration = 0.6f;
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] chord = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C major chord arpeggio

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Clamp((int)(t / 0.12f), 0, chord.Length - 1);
                float freq = chord[noteIndex];

                float phase = 2f * Mathf.PI * freq * t;
                float envelope = 1f - ((float)i / sampleCount);
                samples[i] = Mathf.Sin(phase) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
