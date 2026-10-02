using System.Collections;
using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Base class for all entities placed on the 2D grid.
    /// Manages logical grid coordinates vs interpolated world rendering.
    /// </summary>
    public abstract class GameEntity : MonoBehaviour
    {
        [Header("Grid Coordinates")]
        [SerializeField] protected Vector2Int gridPosition;

        [Header("Visual Interpolation")]
        [SerializeField] protected float moveDuration = 0.12f;

        protected Coroutine moveCoroutine;

        public Vector2Int GridPosition => gridPosition;

        /// <summary>
        /// Sets logical grid position immediately and snaps world transform.
        /// Used during level initialization or undo.
        /// </summary>
        public virtual void SetGridPositionImmediate(Vector2Int newGridPos, Vector3 worldPos)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            gridPosition = newGridPos;
            transform.position = worldPos;
        }

        /// <summary>
        /// Sets logical grid position and starts smooth interpolation towards world target.
        /// </summary>
        public virtual void MoveTo(Vector2Int newGridPos, Vector3 targetWorldPos, float duration = -1f)
        {
            gridPosition = newGridPos;

            if (!Application.isPlaying)
            {
                transform.position = targetWorldPos;
                return;
            }

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            float dur = duration > 0 ? duration : moveDuration;
            moveCoroutine = StartCoroutine(SmoothMoveRoutine(targetWorldPos, dur));
        }

        protected virtual IEnumerator SmoothMoveRoutine(Vector3 targetPos, float duration)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth step interpolation for snappy puzzle feel
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                yield return null;
            }

            transform.position = targetPos;
            moveCoroutine = null;
        }
    }
}
