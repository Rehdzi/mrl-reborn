using Entities;
using UnityEngine;

namespace Generation
{
    [CreateAssetMenu(fileName = "NewBiome", menuName = "Game/Biome")]
    public class BiomeData : ScriptableObject
    {
        [Header("Основная информация")]
        public string biomeName;
        public BiomeType biomeType;
        
        [Header("Климатические параметры")]
        public float temperature;
        public float humidity;
        
        [Header("Допустимые отклонения")]
        [Range(0f, 20f)]
        public float temperatureTolerance = 5f;
        [Range(0f, 30f)]
        public float humidityTolerance = 15f;
        
        [Header("Ресурсы")]
        public GameEntity[] entities;
        public Material[] materials;
        
        /// <summary>
        /// Проверяет, подходит ли данный биом для указанных климатических условий
        /// </summary>
        public bool IsSuitableForEnvironment(float temperature, float humidity)
        {
            float tempDiff = Mathf.Abs(temperature - this.temperature);
            float humidityDiff = Mathf.Abs(humidity - this.humidity);
            
            return tempDiff <= temperatureTolerance && humidityDiff <= humidityTolerance;
        }
        
        /// <summary>
        /// Вычисляет степень совместимости биома с климатическими условиями (0-1)
        /// </summary>
        public float GetCompatibilityScore(float temperature, float humidity)
        {
            float tempScore = 1f - (Mathf.Abs(temperature - this.temperature) / temperatureTolerance);
            float humidityScore = 1f - (Mathf.Abs(humidity - this.humidity) / humidityTolerance);
            
            return Mathf.Clamp01((tempScore + humidityScore) / 2f);
        }
    }
}