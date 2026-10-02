using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Represents a target goal cell where a box must be pushed.
    /// </summary>
    public class TargetController : GameEntity
    {
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 0.9f, 0.85f);
        [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.4f, 1f);
        [SerializeField] private Transform pulseTransform;

        public bool IsOccupied { get; private set; }

        public void Initialize(Vector2Int pos, Vector3 worldPos)
        {
            SetGridPositionImmediate(pos, worldPos);
            SetOccupied(false);
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = occupied ? activeColor : normalColor;
            }

            if (pulseTransform != null)
            {
                pulseTransform.localScale = occupied ? Vector3.one * 1.15f : Vector3.one;
            }
        }
    }
}
