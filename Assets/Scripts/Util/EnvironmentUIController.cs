using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Generation;

namespace Util
{
    /// <summary>
    /// UI контроллер для отображения климатических условий уровня
    /// </summary>
    public class EnvironmentUIController : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private TextMeshProUGUI humidityText;
        [SerializeField] private TextMeshProUGUI levelDepthText;
        [SerializeField] private TextMeshProUGUI biomeText;
        [SerializeField] private Image temperatureIcon;
        [SerializeField] private Image humidityIcon;
        
        [Header("Настройки")]
        [SerializeField] private bool updateInRealTime = true;
        [SerializeField] private float updateInterval = 1f;
        
        private Level currentLevel;
        private float lastUpdateTime;
        
        private void Start()
        {
            // Находим текущий уровень
            currentLevel = Level.levelInstance;
            
            if (currentLevel == null)
            {
                Debug.LogWarning("EnvironmentUIController: Не найден экземпляр Level");
                enabled = false;
                return;
            }
            
            UpdateUI();
        }
        
        private void Update()
        {
            if (updateInRealTime && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateUI();
                lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Обновляет UI с текущими климатическими данными
        /// </summary>
        public void UpdateUI()
        {
            if (currentLevel?.environment == null) return;
            
            var environment = currentLevel.environment;
            
            // Обновляем текстовые поля
            if (temperatureText != null)
            {
                temperatureText.text = $"{environment.CurrentTemperature:F1}°C";
            }
            
            if (humidityText != null)
            {
                humidityText.text = $"{environment.CurrentHumidity:F1}%";
            }
            
            if (levelDepthText != null)
            {
                // Получаем глубину из SessionManager если доступен
                int depth = currentLevel.currentLevelDepth;
                if (SessionManager.instance != null)
                {
                    depth = SessionManager.instance.GetCurrentLevelDepth();
                }
                levelDepthText.text = $"Уровень {depth}";
            }
            
            if (biomeText != null)
            {
                BiomeType biomeType = environment.GetBiomeType();
                biomeText.text = GetBiomeTypeName(biomeType);
            }
            
            // Обновляем иконки (если есть)
            UpdateIcons(environment);
        }
        
        /// <summary>
        /// Обновляет иконки на основе климатических условий
        /// </summary>
        private void UpdateIcons(LevelEnvironment environment)
        {
            if (temperatureIcon != null)
            {
                // Изменяем цвет иконки температуры
                Color tempColor = GetTemperatureColor(environment.CurrentTemperature);
                temperatureIcon.color = tempColor;
            }
            
            if (humidityIcon != null)
            {
                // Изменяем прозрачность иконки влажности
                Color humidityColor = humidityIcon.color;
                humidityColor.a = environment.CurrentHumidity / 100f;
                humidityIcon.color = humidityColor;
            }
        }
        
        /// <summary>
        /// Получает цвет для иконки температуры
        /// </summary>
        private Color GetTemperatureColor(float temperature)
        {
            if (temperature < 0)
                return Color.blue; // Холодно
            if (temperature < 20)
                return Color.cyan; // Прохладно
            if (temperature < 40)
                return Color.green; // Умеренно
            if (temperature < 60)
                return Color.yellow; // Тепло
            return Color.red; // Жарко
        }
        
        /// <summary>
        /// Получает локализованное название типа биома
        /// </summary>
        private string GetBiomeTypeName(BiomeType biomeType)
        {
            switch (biomeType)
            {
                case BiomeType.Frozen: return "Замерзший";
                case BiomeType.ColdWet: return "Холодный влажный";
                case BiomeType.ColdDry: return "Холодный сухой";
                case BiomeType.TemperateWet: return "Умеренный влажный";
                case BiomeType.TemperateDry: return "Умеренный сухой";
                case BiomeType.HotWet: return "Жаркий влажный";
                case BiomeType.HotDry: return "Жаркий сухой";
                case BiomeType.Volcanic: return "Вулканический";
                default: return "Неизвестный";
            }
        }
        
        /// <summary>
        /// Принудительно обновляет UI (вызывается извне)
        /// </summary>
        public void ForceUpdate()
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Переключает режим обновления в реальном времени
        /// </summary>
        public void ToggleRealTimeUpdate()
        {
            updateInRealTime = !updateInRealTime;
        }
        
        /// <summary>
        /// Увеличивает глубину уровня (для тестирования)
        /// </summary>
        public void IncrementLevelDepth()
        {
            if (SessionManager.instance != null)
            {
                SessionManager.instance.IncrementLevelDepth();
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Сбрасывает глубину уровня (для тестирования)
        /// </summary>
        public void ResetLevelDepth()
        {
            if (SessionManager.instance != null)
            {
                SessionManager.instance.ResetLevelDepth();
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Переключает автоматическое увеличение глубины
        /// </summary>
        public void ToggleAutoIncrementDepth()
        {
            if (SessionManager.instance != null)
            {
                SessionManager.instance.ToggleAutoIncrementDepth();
            }
        }
    }
}
