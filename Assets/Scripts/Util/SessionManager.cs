using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionManager : MonoBehaviour
{
    private float _defaultTime = 0.7f;
    
    public GameObject levelPrefab;

    public int level;
    public float globalDifficulty = 1.0f;
    public float localDifficulty;

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
        NewLevel();
        
        debugMap = InputSystem.actions.FindActionMap("Debug");
    }

    IEnumerator GenerateLevel(float time)
    {
        Level levelComponent = GetComponent<Level>();
        if (levelComponent == null && levelPrefab != null)
        {
            levelComponent = levelPrefab.GetComponent<Level>();
        }
        
        yield return new WaitForSeconds(time);
        if (levelComponent == null)
        {
            Debug.LogError("Level component not found on SessionManager or assigned levelPrefab.");
            yield break;
        }
        levelComponent.GenerateLevel();
        localDifficulty = levelComponent.localDifficulty;

    }

    void NewLevel()
    {
        level++;
        globalDifficulty += 0.1f;
        StartCoroutine(GenerateLevel(_defaultTime));
        // Other new level logics
        
        
    }
        
    
    // Update is called once per frame
    void Update()
    {
        if (debugMap.FindAction("New level").WasReleasedThisFrame())
        {
            NewLevel();
        }
    }
}
