using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Represents an immovable wall cell on the grid.
    /// </summary>
    public class WallController : GameEntity
    {
        public void Initialize(Vector2Int pos, Vector3 worldPos)
        {
            SetGridPositionImmediate(pos, worldPos);
        }
    }
}
