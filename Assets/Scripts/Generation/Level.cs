using Generation;
using UnityEngine;
using static MapGenerator;



public class Level : MonoBehaviour
{
    
    public GameObject mapGenerator;
    public float localDifficulty = 1.0f;

    public static Level levelInstance { get; set; }
    
    public void GenerateLocalDifficulty()
    {
        float global = SessionManager.instance.globalDifficulty;
        
        localDifficulty += global + (Random.Range(1.0f, 5.0f) / 10);
    }

    public void GenerateLevel()
    {
        GenerateLocalDifficulty();
        mapGenerator.GetComponent<MapGenerator>().GenerateMap();
        
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
