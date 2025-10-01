using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionManager : MonoBehaviour
{
    private float _defaultTime = 0.7f;
    
    public GameObject levelPrefab;

    public int level = 1;
    public float globalDifficulty = 1.0f;
    public float localDifficulty;
    
    [Header("Система глубины уровня")]
    [SerializeField] private int currentLevelDepth = 1;
    [SerializeField] private bool autoIncrementDepth = true;

    
    [Header("Debug")]
    [SerializeField] public GameObject debugPanel;
    [SerializeField] public bool isDebug = false;
    
    public static SessionManager instance { get; set; }

    private InputActionMap debugMap;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Инициализируем глубину уровня равной начальному уровню
        if (autoIncrementDepth)
        {
            currentLevelDepth = level;
        }
        
        NewLevel();
        
        debugMap = InputSystem.actions.FindActionMap("Debug");
        debugPanel.SetActive(false);
    }

    IEnumerator GenerateLevel(float time)
    {
        Level levelComponent = Level.levelInstance;
        
        yield return new WaitForSeconds(time);
        if (levelComponent == null)
        {
            Debug.LogError("Level component not found on SessionManager or assigned levelPrefab.");
            yield break;
        }
        
        // Устанавливаем глубину уровня перед генерацией
        levelComponent.SetLevelDepth(currentLevelDepth);
        levelComponent.GenerateLevel();
        localDifficulty = levelComponent.localDifficulty;

        // Move camera to start room after level generation
        yield return new WaitForSeconds(0.1f); // Small delay to ensure level generation is complete
        MoveCameraToStartRoom();
    }

    public void NewLevel()
    {
        level++;
        globalDifficulty += 0.1f;
        
        // Глубина уровня должна соответствовать номеру уровня
        if (autoIncrementDepth)
        {
            currentLevelDepth = level;
        }
        
        Debug.Log($"Переход на уровень {level}, глубина: {currentLevelDepth}");
        
        StartCoroutine(GenerateLevel(_defaultTime));
        // Other new level logics
        
        
    }
        
    
    // Update is called once per frame
    void Update()
    {
        if (isDebug)
        {
            if (debugMap.FindAction("New level").WasReleasedThisFrame())
            {
                NewLevel();
            }
        }

        if (debugMap.FindAction("Enter Debug").WasReleasedThisFrame())
        {
            
            if (isDebug)
            {
                isDebug =  false;
                debugPanel.SetActive(false);
            }
            else
            {
                isDebug = true;
                debugPanel.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Получает текущую глубину уровня
    /// </summary>
    public int GetCurrentLevelDepth()
    {
        return currentLevelDepth;
    }
    
    /// <summary>
    /// Устанавливает глубину уровня
    /// </summary>
    public void SetLevelDepth(int depth)
    {
        currentLevelDepth = Mathf.Max(1, depth);
        // Синхронизируем уровень с глубиной если включено автоматическое управление
        if (autoIncrementDepth)
        {
            level = currentLevelDepth;
        }
        Debug.Log($"Глубина уровня установлена на: {currentLevelDepth}");
    }
    
    /// <summary>
    /// Увеличивает глубину уровня на указанное значение
    /// </summary>
    public void IncrementLevelDepth(int amount = 1)
    {
        currentLevelDepth += amount;
        // Синхронизируем уровень с глубиной если включено автоматическое управление
        if (autoIncrementDepth)
        {
            level = currentLevelDepth;
        }
        Debug.Log($"Глубина уровня увеличена до: {currentLevelDepth}");
    }
    
    /// <summary>
    /// Переключает автоматическое увеличение глубины
    /// </summary>
    public void ToggleAutoIncrementDepth()
    {
        autoIncrementDepth = !autoIncrementDepth;
        Debug.Log($"Автоматическое увеличение глубины: {(autoIncrementDepth ? "включено" : "выключено")}");
    }
    
    /// <summary>
    /// Сбрасывает глубину уровня до начального значения
    /// </summary>
    public void ResetLevelDepth()
    {
        currentLevelDepth = 1;
        Debug.Log("Глубина уровня сброшена до 1");
    }

    /// <summary>
    /// Перемещает камеру к стартовой комнате
    /// </summary>
    void MoveCameraToStartRoom()
    {
        // Find RoomGenerator in the scene
        var roomGenerator = FindObjectOfType<Generation.RoomGenerator>();
        if (roomGenerator == null)
        {
            Debug.LogWarning("RoomGenerator not found. Cannot move camera to start room.");
            return;
        }

        // Get start room position
        Vector3 startRoomPosition = roomGenerator.GetStartRoomWorldPosition();
        if (startRoomPosition == Vector3.zero)
        {
            Debug.LogWarning("Start room position not found. Cannot move camera.");
            return;
        }

        // Get camera and move it to start room position
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main camera not found. Cannot move camera.");
            return;
        }

        // Move camera to start room with a slight offset for better viewing
        Vector3 cameraPosition = startRoomPosition + new Vector3(-10, 11, -10); // Offset for better viewing angle
        cam.transform.position = cameraPosition;
        cam.transform.rotation = Quaternion.Euler(45, 45, 0);
        
        // // Make camera look at the start room
        // cam.transform.LookAt(startRoomPosition);
        
        Debug.Log($"Camera moved to start room at {startRoomPosition}");
    }
}
