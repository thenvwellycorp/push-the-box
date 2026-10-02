using System;
using System.Collections;
using UnityEngine;
using PushTheBox.Core;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Controls the player entity on the grid.
    /// Implements IMovable to execute grid movement and box pushing logic.
    /// </summary>
    public class PlayerController : GameEntity, IMovable
    {
        [Header("Visuals & Facing")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform visualTransform;

        public bool IsMoving => moveCoroutine != null;

        public event Action<Vector2Int, bool> OnPlayerMoved; // (newPos, didPushBox)

        public void Initialize(Vector2Int startPos, Vector3 worldPos)
        {
            SetGridPositionImmediate(startPos, worldPos);
            if (visualTransform != null)
            {
                visualTransform.localScale = Vector3.one;
            }
        }

        public bool TryMove(Vector2Int direction)
        {
            // Only process moves while playing and when not already moving
            if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying())
                return false;

            if (IsMoving)
                return false;

            if (direction == Vector2Int.zero)
                return false;

            // Normalize to cardinal directions
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                direction = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
            else
                direction = new Vector2Int(0, direction.y > 0 ? 1 : -1);

            UpdateFacing(direction);

            Vector2Int destination = gridPosition + direction;

            // 1. Check for wall collision
            if (GridManager.Instance != null && GridManager.Instance.IsWall(destination))
            {
                if (Application.isPlaying) StartCoroutine(BumpFeedbackRoutine(direction));
                return false; // Invalid move, not counted
            }

            // 2. Check for box collision
            BoxController box = GridManager.Instance != null ? GridManager.Instance.GetBoxAt(destination) : null;
            bool pushedBox = false;

            if (box != null)
            {
                if (!box.CanPush(direction))
                {
                    if (Application.isPlaying) StartCoroutine(BumpFeedbackRoutine(direction));
                    return false; // Box blocked, invalid move
                }

                // Push the box
                box.Push(direction);
                pushedBox = true;
            }

            // 3. Move player into target cell
            Vector3 targetWorldPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(destination) : (Vector3)(Vector2)destination;
            MoveTo(destination, targetWorldPos, moveDuration);

            // Trigger squash/stretch animation
            if (Application.isPlaying) StartCoroutine(SquashStretchRoutine());

            OnPlayerMoved?.Invoke(destination, pushedBox);
            return true;
        }

        private void UpdateFacing(Vector2Int dir)
        {
            if (visualTransform == null) return;

            if (dir.x != 0)
            {
                // Flip X for left/right
                Vector3 scale = visualTransform.localScale;
                scale.x = dir.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }
        }

        private IEnumerator SquashStretchRoutine()
        {
            if (visualTransform == null) yield break;

            Vector3 normalScale = new Vector3(Mathf.Sign(visualTransform.localScale.x), 1f, 1f);
            Vector3 squashScale = new Vector3(normalScale.x * 1.15f, 0.85f, 1f);

            float dur = moveDuration * 0.5f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                visualTransform.localScale = Vector3.Lerp(normalScale, squashScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                visualTransform.localScale = Vector3.Lerp(squashScale, normalScale, t);
                yield return null;
            }

            visualTransform.localScale = normalScale;
        }

        private IEnumerator BumpFeedbackRoutine(Vector2Int dir)
        {
            if (visualTransform == null) yield break;

            Vector3 startPos = visualTransform.localPosition;
            Vector3 bumpOffset = new Vector3(dir.x * 0.12f, dir.y * 0.12f, 0f);

            float dur = 0.08f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                visualTransform.localPosition = Vector3.Lerp(startPos, startPos + bumpOffset, elapsed / dur);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                visualTransform.localPosition = Vector3.Lerp(startPos + bumpOffset, startPos, elapsed / dur);
                yield return null;
            }

            visualTransform.localPosition = startPos;
        }
    }
}
