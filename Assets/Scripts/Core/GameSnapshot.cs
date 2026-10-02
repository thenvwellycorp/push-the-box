using System.Collections.Generic;
using UnityEngine;

namespace PushTheBox.Core
{
    /// <summary>
    /// Immutable record capturing state at a single step for the undo system (Memento pattern).
    /// </summary>
    public class GameSnapshot
    {
        public Vector2Int PlayerPosition { get; }
        public IReadOnlyDictionary<int, Vector2Int> BoxPositions { get; }
        public int MoveCount { get; }

        public GameSnapshot(Vector2Int playerPos, IDictionary<int, Vector2Int> boxPositions, int moveCount)
        {
            PlayerPosition = playerPos;
            BoxPositions = new Dictionary<int, Vector2Int>(boxPositions);
            MoveCount = moveCount;
        }
    }
}
