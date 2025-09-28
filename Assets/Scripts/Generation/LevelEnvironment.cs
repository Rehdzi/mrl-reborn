using UnityEngine;

namespace Generation
{
    /// <summary>
    /// Система управления климатическими параметрами уровня подземелья
    /// </summary>
    [System.Serializable]
    public class LevelEnvironment
    {
        [Header("Базовые параметры")]
        [SerializeField] private float baseTemperature = 20f; // Базовая температура на поверхности
        [SerializeField] private float baseHumidity = 50f; // Базовая влажность на поверхности
        
        [Header("Влияние глубины")]
        [SerializeField] private float temperatureIncreasePerLevel = 2.5f; // Увеличение температуры на каждый уровень
        [SerializeField] private float humidityDecreasePerLevel = 1f; // Уменьшение влажности на каждый уровень
        
        [Header("Случайные вариации")]
        [SerializeField] private float temperatureVariation = 5f; // Максимальное отклонение температуры
        [SerializeField] private float humidityVariation = 15f; // Максимальное отклонение влажности
        [SerializeField] private float variationChance = 0.3f; // Вероятность случайного изменения (0-1)
        
        [Header("Ограничения")]
        [SerializeField] private float minTemperature = -10f;
        [SerializeField] private float maxTemperature = 100f;
        [SerializeField] private float minHumidity = 0f;
        [SerializeField] private float maxHumidity = 100f;
        
        // Текущие значения
        public float CurrentTemperature { get; private set; }
        public float CurrentHumidity { get; private set; }
        
        /// <summary>
        /// Инициализирует климатические параметры для указанного уровня
        /// </summary>
        /// <param name="levelDepth">Глубина уровня (начинается с 1)</param>
        /// <param name="seed">Сид для воспроизводимости</param>
        public void InitializeEnvironment(int levelDepth, int seed = 0)
        {
            Random.InitState(seed);
            
            // Расчет базовой температуры с учетом глубины
            float depthTemperature = baseTemperature + (levelDepth - 1) * temperatureIncreasePerLevel;
            
            // Расчет базовой влажности с учетом глубины
            float depthHumidity = Mathf.Max(0, baseHumidity - (levelDepth - 1) * humidityDecreasePerLevel);
            
            // Применение случайных вариаций
            CurrentTemperature = CalculateWithVariation(depthTemperature, temperatureVariation, variationChance);
            CurrentHumidity = CalculateWithVariation(depthHumidity, humidityVariation, variationChance);
            
            // Ограничение значений
            CurrentTemperature = Mathf.Clamp(CurrentTemperature, minTemperature, maxTemperature);
            CurrentHumidity = Mathf.Clamp(CurrentHumidity, minHumidity, maxHumidity);
        }
        
        /// <summary>
        /// Вычисляет значение с возможными случайными вариациями
        /// </summary>
        private float CalculateWithVariation(float baseValue, float variation, float chance)
        {
            if (Random.value < chance)
            {
                // Применяем случайное отклонение
                float randomVariation = Random.Range(-variation, variation);
                return baseValue + randomVariation;
            }
            
            return baseValue;
        }
        
        /// <summary>
        /// Обновляет климатические параметры (может быть вызвано для динамических изменений)
        /// </summary>
        public void UpdateEnvironment(int levelDepth, int seed = 0)
        {
            InitializeEnvironment(levelDepth, seed);
        }
        
        /// <summary>
        /// Получает описание климатических условий уровня
        /// </summary>
        public string GetEnvironmentDescription()
        {
            string tempDescription = GetTemperatureDescription(CurrentTemperature);
            string humidityDescription = GetHumidityDescription(CurrentHumidity);
            
            return $"Температура: {CurrentTemperature:F1}°C ({tempDescription})\n" +
                   $"Влажность: {CurrentHumidity:F1}% ({humidityDescription})";
        }
        
        private string GetTemperatureDescription(float temperature)
        {
            if (temperature < 0) return "Очень холодно";
            if (temperature < 10) return "Холодно";
            if (temperature < 20) return "Прохладно";
            if (temperature < 30) return "Умеренно";
            if (temperature < 40) return "Тепло";
            if (temperature < 60) return "Жарко";
            return "Очень жарко";
        }
        
        private string GetHumidityDescription(float humidity)
        {
            if (humidity < 20) return "Очень сухо";
            if (humidity < 40) return "Сухо";
            if (humidity < 60) return "Умеренно";
            if (humidity < 80) return "Влажно";
            return "Очень влажно";
        }
        
        /// <summary>
        /// Получает биом на основе климатических условий
        /// </summary>
        public BiomeType GetBiomeType()
        {
            if (CurrentTemperature < 0)
                return BiomeType.Frozen;
            if (CurrentTemperature < 15 && CurrentHumidity > 60)
                return BiomeType.ColdWet;
            if (CurrentTemperature < 15)
                return BiomeType.ColdDry;
            if (CurrentTemperature < 30 && CurrentHumidity > 70)
                return BiomeType.TemperateWet;
            if (CurrentTemperature < 30)
                return BiomeType.TemperateDry;
            if (CurrentTemperature < 50 && CurrentHumidity > 60)
                return BiomeType.HotWet;
            if (CurrentTemperature < 50)
                return BiomeType.HotDry;
            return BiomeType.Volcanic;
        }
    }
    
    /// <summary>
    /// Типы биомов на основе климатических условий
    /// </summary>
    public enum BiomeType
    {
        Frozen,      // Замерзший
        ColdWet,     // Холодный влажный
        ColdDry,     // Холодный сухой
        TemperateWet, // Умеренный влажный
        TemperateDry, // Умеренный сухой
        HotWet,      // Жаркий влажный
        HotDry,      // Жаркий сухой
        Volcanic     // Вулканический
    }
}
