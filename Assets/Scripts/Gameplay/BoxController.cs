using System;
using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Controls a pushable box entity on the grid.
    /// Implements IPushable to decouple player push mechanics from box internals.
    /// </summary>
    public class BoxController : GameEntity, IPushable
    {
        [Header("Box Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new Color(0.95f, 0.72f, 0.35f, 1f); // Warm wood
        [SerializeField] private Color onTargetColor = new Color(0.35f, 0.95f, 0.45f, 1f); // Vibrant emerald/gold
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite onTargetSprite;

        public int BoxId { get; private set; }
        public bool IsOnTarget { get; private set; }

        public event Action<BoxController, bool> OnTargetStatusChanged;

        public void Initialize(int id, Vector2Int startPos, Vector3 worldPos)
        {
            BoxId = id;
            SetGridPositionImmediate(startPos, worldPos);
            RefreshTargetVisual(false);
        }

        public bool CanPush(Vector2Int direction)
        {
            if (GridManager.Instance == null) return false;
            Vector2Int destination = gridPosition + direction;
            return GridManager.Instance.IsCellWalkable(destination);
        }

        public void Push(Vector2Int direction)
        {
            Vector2Int oldPos = gridPosition;
            Vector2Int newPos = gridPosition + direction;

            if (GridManager.Instance != null)
            {
                GridManager.Instance.UpdateBoxPosition(this, oldPos, newPos);
            }
            Vector3 targetWorldPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(newPos) : (Vector3)(Vector2)newPos;

            MoveTo(newPos, targetWorldPos, moveDuration);
            RefreshTargetVisual(true);
        }

        public void RestorePosition(Vector2Int restoredPos, Vector3 restoredWorldPos)
        {
            Vector2Int oldPos = gridPosition;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.UpdateBoxPosition(this, oldPos, restoredPos);
            }
            SetGridPositionImmediate(restoredPos, restoredWorldPos);
            RefreshTargetVisual(false);
        }

        public void RefreshTargetVisual(bool triggerPunch = false)
        {
            bool wasOnTarget = IsOnTarget;
            IsOnTarget = GridManager.Instance != null && GridManager.Instance.IsTarget(gridPosition);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = IsOnTarget ? onTargetColor : normalColor;
                if (onTargetSprite != null && normalSprite != null)
                {
                    spriteRenderer.sprite = IsOnTarget ? onTargetSprite : normalSprite;
                }
            }

            TargetController target = GridManager.Instance != null ? GridManager.Instance.GetTargetAt(gridPosition) : null;
            if (target != null)
            {
                target.SetOccupied(IsOnTarget);
            }

            if (triggerPunch && IsOnTarget && !wasOnTarget)
            {
                // Small bounce punch scale when landing on target
                if (Application.isPlaying) StartCoroutine(PunchScaleRoutine());
            }

            if (wasOnTarget != IsOnTarget)
            {
                OnTargetStatusChanged?.Invoke(this, IsOnTarget);
            }
        }

        private System.Collections.IEnumerator PunchScaleRoutine()
        {
            Vector3 originalScale = Vector3.one;
            Vector3 punchScale = Vector3.one * 1.2f;
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}
