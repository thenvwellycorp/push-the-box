using System;
using UnityEngine;
using UnityEngine.UI;

namespace PushTheBox.UI
{
    /// <summary>
    /// Represents an individual level card on the Level Select screen.
    /// </summary>
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private Text levelNumberText;
        [SerializeField] private Text starsText;
        [SerializeField] private GameObject lockOverlay;

        private int targetLevelId;
        private Action<int> onSelectAction;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (levelNumberText == null || starsText == null)
            {
                var texts = GetComponentsInChildren<Text>(true);
                if (levelNumberText == null && texts.Length > 0) levelNumberText = texts[0];
                if (starsText == null && texts.Length > 1) starsText = texts[1];
            }

            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
                button.onClick.AddListener(OnClick);
            }
        }

        public void Bind(Button btn, Text numTxt, Text starTxt, GameObject lockObj)
        {
            button = btn;
            levelNumberText = numTxt;
            starsText = starTxt;
            lockOverlay = lockObj;

            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
                button.onClick.AddListener(OnClick);
            }
        }

        public void Setup(int levelId, bool isUnlocked, int stars, Action<int> onSelect)
        {
            targetLevelId = levelId;
            onSelectAction = onSelect;

            if (levelNumberText != null)
            {
                levelNumberText.text = $"{levelId}";
            }

            if (button != null)
            {
                button.interactable = isUnlocked;
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);
            }

            if (starsText != null)
            {
                if (!isUnlocked)
                {
                    starsText.text = "";
                }
                else
                {
                    // Render star glyphs
                    string starsStr = "";
                    for (int s = 0; s < 3; s++)
                    {
                        starsStr += (s < stars) ? "★" : "☆";
                    }
                    starsText.text = starsStr;
                }
            }
        }

        private void OnClick()
        {
            onSelectAction?.Invoke(targetLevelId);
        }
    }
}
