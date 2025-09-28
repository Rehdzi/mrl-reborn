using UnityEngine;

namespace Generation
{
    /// <summary>
    /// Адаптер для изменения параметров генерации уровня в зависимости от биома
    /// </summary>
    public static class BiomeGenerationAdapter
    {
        /// <summary>
        /// Получает модификаторы генерации для указанного типа биома
        /// </summary>
        public static GenerationModifiers GetModifiersForBiome(BiomeType biomeType)
        {
            var modifiers = new GenerationModifiers();
            
            switch (biomeType)
            {
                case BiomeType.Frozen:
                    ApplyFrozenModifiers(modifiers);
                    break;
                case BiomeType.ColdWet:
                    ApplyColdWetModifiers(modifiers);
                    break;
                case BiomeType.ColdDry:
                    ApplyColdDryModifiers(modifiers);
                    break;
                case BiomeType.TemperateWet:
                    ApplyTemperateWetModifiers(modifiers);
                    break;
                case BiomeType.TemperateDry:
                    ApplyTemperateDryModifiers(modifiers);
                    break;
                case BiomeType.HotWet:
                    ApplyHotWetModifiers(modifiers);
                    break;
                case BiomeType.HotDry:
                    ApplyHotDryModifiers(modifiers);
                    break;
                case BiomeType.Volcanic:
                    ApplyVolcanicModifiers(modifiers);
                    break;
            }
            
            return modifiers;
        }
        
        /// <summary>
        /// Применяет модификаторы для замерзшего биома
        /// </summary>
        private static void ApplyFrozenModifiers(GenerationModifiers modifiers)
        {
            // Замерзшие области: меньше комнат, больше открытого пространства
            modifiers.roomsCountMultiplier = 0.6f;
            modifiers.roomSizeMinMultiplier = new Vector2(1.5f, 1.5f);
            modifiers.roomSizeMaxMultiplier = new Vector2(2f, 2f);
            modifiers.roomThresholdSizeMultiplier = 0.7f;
            modifiers.wallThresholdSizeMultiplier = 0.8f;
            modifiers.corridorRadiusMultiplier = 1.2f;
            modifiers.randomFillPercentMultiplier = 0.8f;
        }
        
        /// <summary>
        /// Применяет модификаторы для холодного влажного биома
        /// </summary>
        private static void ApplyColdWetModifiers(GenerationModifiers modifiers)
        {
            // Влажные холодные области: средние комнаты, больше проходов
            modifiers.roomsCountMultiplier = 0.8f;
            modifiers.roomSizeMinMultiplier = new Vector2(1.2f, 1.2f);
            modifiers.roomSizeMaxMultiplier = new Vector2(1.5f, 1.5f);
            modifiers.corridorRadiusMultiplier = 1.3f;
            modifiers.passageRadiusMultiplier = 1.2f;
            modifiers.randomFillPercentMultiplier = 0.9f;
        }
        
        /// <summary>
        /// Применяет модификаторы для холодного сухого биома
        /// </summary>
        private static void ApplyColdDryModifiers(GenerationModifiers modifiers)
        {
            // Сухие холодные области: компактные комнаты, меньше проходов
            modifiers.roomsCountMultiplier = 1.2f;
            modifiers.roomSizeMinMultiplier = new Vector2(0.8f, 0.8f);
            modifiers.roomSizeMaxMultiplier = new Vector2(1.1f, 1.1f);
            modifiers.corridorRadiusMultiplier = 0.8f;
            modifiers.passageRadiusMultiplier = 0.9f;
            modifiers.mergeThresholdMultiplier = 1.2f;
        }
        
        /// <summary>
        /// Применяет модификаторы для умеренного влажного биома
        /// </summary>
        private static void ApplyTemperateWetModifiers(GenerationModifiers modifiers)
        {
            // Умеренные влажные области: стандартные параметры с небольшими изменениями
            modifiers.roomsCountMultiplier = 1.0f;
            modifiers.roomSizeMinMultiplier = new Vector2(1.0f, 1.0f);
            modifiers.roomSizeMaxMultiplier = new Vector2(1.1f, 1.1f);
            modifiers.corridorRadiusMultiplier = 1.1f;
            modifiers.randomFillPercentMultiplier = 1.0f;
        }
        
        /// <summary>
        /// Применяет модификаторы для умеренного сухого биома
        /// </summary>
        private static void ApplyTemperateDryModifiers(GenerationModifiers modifiers)
        {
            // Умеренные сухие области: стандартные параметры
            modifiers.roomsCountMultiplier = 1.0f;
            modifiers.roomSizeMinMultiplier = new Vector2(1.0f, 1.0f);
            modifiers.roomSizeMaxMultiplier = new Vector2(1.0f, 1.0f);
            modifiers.corridorRadiusMultiplier = 1.0f;
            modifiers.randomFillPercentMultiplier = 1.0f;
        }
        
        /// <summary>
        /// Применяет модификаторы для жаркого влажного биома
        /// </summary>
        private static void ApplyHotWetModifiers(GenerationModifiers modifiers)
        {
            // Жаркие влажные области: большие открытые пространства, меньше стен
            modifiers.roomsCountMultiplier = 0.7f;
            modifiers.roomSizeMinMultiplier = new Vector2(1.3f, 1.3f);
            modifiers.roomSizeMaxMultiplier = new Vector2(1.8f, 1.8f);
            modifiers.roomThresholdSizeMultiplier = 1.2f;
            modifiers.wallThresholdSizeMultiplier = 1.3f;
            modifiers.corridorRadiusMultiplier = 1.4f;
            modifiers.randomFillPercentMultiplier = 0.7f;
            modifiers.smoothIterationsMultiplier = 0.8f;
        }
        
        /// <summary>
        /// Применяет модификаторы для жаркого сухого биома
        /// </summary>
        private static void ApplyHotDryModifiers(GenerationModifiers modifiers)
        {
            // Жаркие сухие области: много маленьких комнат, узкие проходы
            modifiers.roomsCountMultiplier = 1.5f;
            modifiers.roomSizeMinMultiplier = new Vector2(0.6f, 0.6f);
            modifiers.roomSizeMaxMultiplier = new Vector2(0.9f, 0.9f);
            modifiers.corridorRadiusMultiplier = 0.7f;
            modifiers.passageRadiusMultiplier = 0.8f;
            modifiers.mergeThresholdMultiplier = 1.3f;
            modifiers.placementAttemptsMultiplier = 1.2f;
        }
        
        /// <summary>
        /// Применяет модификаторы для вулканического биома
        /// </summary>
        private static void ApplyVolcanicModifiers(GenerationModifiers modifiers)
        {
            // Вулканические области: хаотичная структура, большие и маленькие комнаты
            modifiers.roomsCountMultiplier = 1.3f;
            modifiers.roomSizeMinMultiplier = new Vector2(0.5f, 0.5f);
            modifiers.roomSizeMaxMultiplier = new Vector2(2.5f, 2.5f);
            modifiers.roomThresholdSizeMultiplier = 0.6f;
            modifiers.wallThresholdSizeMultiplier = 0.7f;
            modifiers.corridorRadiusMultiplier = 1.5f;
            modifiers.passageRadiusMultiplier = 1.3f;
            modifiers.randomFillPercentMultiplier = 0.6f;
            modifiers.smoothIterationsMultiplier = 1.2f;
            modifiers.placementAttemptsMultiplier = 1.5f;
        }
        
        /// <summary>
        /// Получает описание влияния биома на генерацию
        /// </summary>
        public static string GetBiomeGenerationDescription(BiomeType biomeType)
        {
            switch (biomeType)
            {
                case BiomeType.Frozen:
                    return "Замерзшие области: меньше комнат, больше открытого пространства";
                case BiomeType.ColdWet:
                    return "Холодные влажные области: средние комнаты, больше проходов";
                case BiomeType.ColdDry:
                    return "Холодные сухие области: компактные комнаты, меньше проходов";
                case BiomeType.TemperateWet:
                    return "Умеренные влажные области: стандартные параметры с небольшими изменениями";
                case BiomeType.TemperateDry:
                    return "Умеренные сухие области: стандартные параметры";
                case BiomeType.HotWet:
                    return "Жаркие влажные области: большие открытые пространства, меньше стен";
                case BiomeType.HotDry:
                    return "Жаркие сухие области: много маленьких комнат, узкие проходы";
                case BiomeType.Volcanic:
                    return "Вулканические области: хаотичная структура, большие и маленькие комнаты";
                default:
                    return "Неизвестный биом";
            }
        }
    }
}
