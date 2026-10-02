using UnityEngine;

namespace PushTheBox.Level
{
    /// <summary>
    /// ScriptableObject defining the configuration, layout, and star thresholds for a puzzle level.
    /// Data-driven approach allows easily creating and tuning levels without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "PushTheBox/Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Information")]
        public int levelId = 1;
        public string levelName = "Level 1";

        [Header("Board Dimensions")]
        public int width = 5;
        public int height = 5;

        [Header("Entity Positions (Grid Coordinates)")]
        public Vector2Int playerPosition;
        public Vector2Int[] walls = new Vector2Int[0];
        public Vector2Int[] boxes = new Vector2Int[0];
        public Vector2Int[] targets = new Vector2Int[0];

        [Header("Star Rating Thresholds (Moves)")]
        [Tooltip("Moves <= this amount awards 3 stars")]
        public int threeStarMoves = 10;
        [Tooltip("Moves <= this amount awards 2 stars")]
        public int twoStarMoves = 15;

        [Header("Coin Rewards")]
        [Tooltip("Base coins awarded for completing the level")]
        public int baseCoinReward = 50;
        [Tooltip("Bonus coins awarded per star earned")]
        public int bonusCoinPerStar = 10;

        /// <summary>
        /// Calculates star rating based on the player's move count.
        /// </summary>
        public int CalculateStars(int moves)
        {
            if (moves <= threeStarMoves) return 3;
            if (moves <= twoStarMoves) return 2;
            return 1;
        }

        /// <summary>
        /// Calculates total coin reward based on star rating achieved.
        /// </summary>
        public int CalculateCoinReward(int stars)
        {
            return Mathf.Max(0, baseCoinReward + (stars * bonusCoinPerStar));
        }

        private void OnValidate()
        {
            if (width < 3) width = 3;
            if (height < 3) height = 3;
            if (twoStarMoves < threeStarMoves) twoStarMoves = threeStarMoves + 5;
            if (baseCoinReward < 0) baseCoinReward = 0;
            if (bonusCoinPerStar < 0) bonusCoinPerStar = 0;
        }
    }
}
