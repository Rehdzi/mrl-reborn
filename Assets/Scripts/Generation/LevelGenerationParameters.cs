using UnityEngine;

namespace Generation
{
    /// <summary>
    /// Параметры генерации уровня, адаптированные под биом
    /// </summary>
    [System.Serializable]
    public class LevelGenerationParameters
    {
        [Header("Параметры карты")]
        [SerializeField] public int randomFillPercent = 45;
        [SerializeField] public int smoothIterations = 5;
        [SerializeField] public int wallThresholdSize = 50;
        [SerializeField] public int roomThresholdSize = 50;
        
        [Header("Параметры комнат")]
        [SerializeField] public int roomsCount = 7;
        [SerializeField] public Vector2Int roomSizeMin = new Vector2Int(6, 6);
        [SerializeField] public Vector2Int roomSizeMax = new Vector2Int(14, 14);
        [SerializeField] public int placementAttempts = 40;
        [SerializeField] public int corridorRadius = 2;
        [SerializeField] public int mergeThreshold = 3;
        [SerializeField] public float roomHeight = 5f;
        
        [Header("Параметры проходов")]
        [SerializeField] public int passageRadius = 1;
        [SerializeField] public float caveSquareSize = 1f;
        
        
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public LevelGenerationParameters()
        {
            // Значения по умолчанию уже установлены в полях
        }
        
        /// <summary>
        /// Создает копию параметров с возможностью модификации
        /// </summary>
        public LevelGenerationParameters Clone()
        {
            return new LevelGenerationParameters
            {
                randomFillPercent = this.randomFillPercent,
                smoothIterations = this.smoothIterations,
                wallThresholdSize = this.wallThresholdSize,
                roomThresholdSize = this.roomThresholdSize,
                roomsCount = this.roomsCount,
                roomSizeMin = this.roomSizeMin,
                roomSizeMax = this.roomSizeMax,
                placementAttempts = this.placementAttempts,
                corridorRadius = this.corridorRadius,
                mergeThreshold = this.mergeThreshold,
                roomHeight = this.roomHeight,
                passageRadius = this.passageRadius,
                caveSquareSize = this.caveSquareSize
            };
        }
        
        /// <summary>
        /// Применяет модификаторы к параметрам
        /// </summary>
        public void ApplyModifiers(GenerationModifiers modifiers)
        {
            randomFillPercent = Mathf.RoundToInt(randomFillPercent * modifiers.randomFillPercentMultiplier);
            smoothIterations = Mathf.RoundToInt(smoothIterations * modifiers.smoothIterationsMultiplier);
            wallThresholdSize = Mathf.RoundToInt(wallThresholdSize * modifiers.wallThresholdSizeMultiplier);
            roomThresholdSize = Mathf.RoundToInt(roomThresholdSize * modifiers.roomThresholdSizeMultiplier);
            roomsCount = Mathf.RoundToInt(roomsCount * modifiers.roomsCountMultiplier);
            
            roomSizeMin = new Vector2Int(
                Mathf.RoundToInt(roomSizeMin.x * modifiers.roomSizeMinMultiplier.x),
                Mathf.RoundToInt(roomSizeMin.y * modifiers.roomSizeMinMultiplier.y)
            );
            
            roomSizeMax = new Vector2Int(
                Mathf.RoundToInt(roomSizeMax.x * modifiers.roomSizeMaxMultiplier.x),
                Mathf.RoundToInt(roomSizeMax.y * modifiers.roomSizeMaxMultiplier.y)
            );
            
            placementAttempts = Mathf.RoundToInt(placementAttempts * modifiers.placementAttemptsMultiplier);
            corridorRadius = Mathf.RoundToInt(corridorRadius * modifiers.corridorRadiusMultiplier);
            mergeThreshold = Mathf.RoundToInt(mergeThreshold * modifiers.mergeThresholdMultiplier);
            roomHeight *= modifiers.roomHeightMultiplier;
            passageRadius = Mathf.RoundToInt(passageRadius * modifiers.passageRadiusMultiplier);
            caveSquareSize *= modifiers.caveSquareSizeMultiplier;
        }
    }
    
    /// <summary>
    /// Модификаторы для адаптации параметров генерации под биом
    /// </summary>
    [System.Serializable]
    public class GenerationModifiers
    {
        [Header("Модификаторы карты")]
        [Range(0.1f, 3f)] public float randomFillPercentMultiplier = 1f;
        [Range(0.1f, 3f)] public float smoothIterationsMultiplier = 1f;
        [Range(0.1f, 3f)] public float wallThresholdSizeMultiplier = 1f;
        [Range(0.1f, 3f)] public float roomThresholdSizeMultiplier = 1f;
        
        [Header("Модификаторы комнат")]
        [Range(0.1f, 3f)] public float roomsCountMultiplier = 1f;
        public Vector2 roomSizeMinMultiplier = Vector2.one;
        public Vector2 roomSizeMaxMultiplier = Vector2.one;
        [Range(0.1f, 3f)] public float placementAttemptsMultiplier = 1f;
        [Range(0.1f, 3f)] public float corridorRadiusMultiplier = 1f;
        [Range(0.1f, 3f)] public float mergeThresholdMultiplier = 1f;
        [Range(0.1f, 3f)] public float roomHeightMultiplier = 1f;
        
        [Header("Модификаторы проходов")]
        [Range(0.1f, 3f)] public float passageRadiusMultiplier = 1f;
        [Range(0.1f, 3f)] public float caveSquareSizeMultiplier = 1f;
    }
}
