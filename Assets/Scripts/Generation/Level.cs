using Generation;
using UnityEngine;
using static MapGenerator;



public class Level : MonoBehaviour
{
    
    public GameObject mapGenerator;
    public float localDifficulty = 1.0f;
    
    [Header("Климатические параметры")]
    public LevelEnvironment environment = new LevelEnvironment();
    public int currentLevelDepth = 1; // Глубина текущего уровня (начинается с 1)

    public static Level levelInstance { get; set; }
    
    public void GenerateLocalDifficulty()
    {
        float global = SessionManager.instance.globalDifficulty;
        
        localDifficulty += global + (Random.Range(1.0f, 5.0f) / 10);
    }

    public void GenerateLevel()
    {
        GenerateLocalDifficulty();
        GenerateEnvironment();
        mapGenerator.GetComponent<MapGenerator>().GenerateMap();
        
    }
    
    /// <summary>
    /// Генерирует климатические параметры для текущего уровня
    /// </summary>
    public void GenerateEnvironment()
    {
        // Получаем глубину из SessionManager если доступен
        if (SessionManager.instance != null)
        {
            currentLevelDepth = SessionManager.instance.GetCurrentLevelDepth();
        }
        
        // Используем комбинацию времени и глубины для создания уникального сида
        int environmentSeed = (int)(Time.time * 1000) + currentLevelDepth * 1000;
        environment.InitializeEnvironment(currentLevelDepth, environmentSeed);
        
        Debug.Log($"Уровень {currentLevelDepth}: {environment.GetEnvironmentDescription()}");
    }
    
    /// <summary>
    /// Устанавливает глубину уровня (вызывается при переходе на новый уровень)
    /// </summary>
    public void SetLevelDepth(int depth)
    {
        currentLevelDepth = Mathf.Max(1, depth);
    }
    
    private void Awake()
    {
        if (levelInstance != null && levelInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            levelInstance = this;
        }
    }
    
    void Start()
    {
        
    }
}
