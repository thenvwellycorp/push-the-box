using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;

namespace PushTheBox.UI
{
    /// <summary>
    /// Modal dialog displayed upon level completion.
    /// Shows moves, star rating animation, and replay/next buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [Header("Modal Containers")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Transform dialogContent;

        [Header("Information Labels")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text movesText;

        [Header("Star Icons")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Color starActiveColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color starInactiveColor = new Color(0.35f, 0.35f, 0.4f, 0.5f);

        [Header("Action Buttons")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button levelSelectButton;

        private void OnEnable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            }

            if (replayButton != null) replayButton.onClick.AddListener(OnReplayClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);

            if (modalRoot != null)
                modalRoot.SetActive(false);
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }

            if (replayButton != null) replayButton.onClick.RemoveListener(OnReplayClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }

        private void HandleLevelCompleted(LevelCompletionData data)
        {
            if (modalRoot != null)
                modalRoot.SetActive(true);

            if (titleText != null)
                titleText.text = $"{data.levelName} Cleared!";

            if (movesText != null)
                movesText.text = $"Total Moves: {data.moves}";

            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(data.hasNextLevel);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.LevelComplete);
            }

            StartCoroutine(AnimateStarsRoutine(data.stars));
        }

        private IEnumerator AnimateStarsRoutine(int starsEarned)
        {
            // Reset all stars to inactive
            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].color = starInactiveColor;
                        starImages[i].transform.localScale = Vector3.one * 0.8f;
                    }
                }

                yield return new WaitForSeconds(0.2f);

                // Reveal stars sequentially with pop bounce
                for (int i = 0; i < starImages.Length && i < starsEarned; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].color = starActiveColor;

                        // Punch bounce
                        float dur = 0.2f;
                        float elapsed = 0f;
                        while (elapsed < dur)
                        {
                            elapsed += Time.deltaTime;
                            float t = elapsed / dur;
                            float scale = Mathf.Lerp(1.5f, 1f, t);
                            starImages[i].transform.localScale = Vector3.one * scale;
                            yield return null;
                        }
                        starImages[i].transform.localScale = Vector3.one;

                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySound(SoundType.BoxOnTarget);
                        }

                        yield return new WaitForSeconds(0.12f);
                    }
                }
            }
        }

        private void OnReplayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (modalRoot != null) modalRoot.SetActive(false);
            if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
        }

        private void OnNextLevelClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (modalRoot != null) modalRoot.SetActive(false);
            if (LevelManager.Instance != null) LevelManager.Instance.LoadNextLevel();
        }

        private void OnLevelSelectClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (modalRoot != null) modalRoot.SetActive(false);
            if (GameStateManager.Instance != null) GameStateManager.Instance.SetState(GameState.LevelSelect);
        }
    }
}
