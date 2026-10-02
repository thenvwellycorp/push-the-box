using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;
using PushTheBox.Save;

namespace PushTheBox.UI
{
    /// <summary>
    /// Controls the title / main menu screen.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Text soundToggleText;

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.AddListener(OnSoundToggleClicked);

            UpdateSoundUI();
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.RemoveListener(OnSoundToggleClicked);
        }

        private void OnPlayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);

            int targetLevel = 1;
            if (SaveManager.Instance != null)
            {
                targetLevel = SaveManager.Instance.GetHighestUnlockedLevel();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(targetLevel);
            }
        }

        private void OnLevelSelectClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.LevelSelect);
            }
        }

        private void OnSoundToggleClicked()
        {
            if (AudioManager.Instance != null)
            {
                bool newMuted = !AudioManager.Instance.IsMuted;
                AudioManager.Instance.SetMuted(newMuted);
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
            UpdateSoundUI();
        }

        private void UpdateSoundUI()
        {
            if (soundToggleText != null && AudioManager.Instance != null)
            {
                soundToggleText.text = AudioManager.Instance.IsMuted ? "Sound: OFF" : "Sound: ON";
            }
        }
    }
}
