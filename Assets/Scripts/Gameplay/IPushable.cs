using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Contract for any entity that can be pushed on the grid (e.g. Boxes).
    /// </summary>
    public interface IPushable
    {
        Vector2Int GridPosition { get; }

        /// <summary>
        /// Checks whether this entity can be pushed in the specified direction.
        /// </summary>
        bool CanPush(Vector2Int direction);

        /// <summary>
        /// Executes the push in the specified direction.
        /// </summary>
        void Push(Vector2Int direction);
    }
}
