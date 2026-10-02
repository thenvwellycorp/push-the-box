using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Contract for grid movable entities (e.g. Player).
    /// </summary>
    public interface IMovable
    {
        Vector2Int GridPosition { get; }

        /// <summary>
        /// Attempts to move the entity by the given grid direction vector.
        /// Returns true if the move was successful.
        /// </summary>
        bool TryMove(Vector2Int direction);
    }
}
