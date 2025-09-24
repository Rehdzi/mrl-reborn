using Generation;
using UnityEngine;
using static MapGenerator;



public class Level : MapGenerator
{
    
    public float localDifficulty = 1.0f;

    public void GenerateLocalDifficulty()
    {
        float global = SessionManager.instance.globalDifficulty;
        
        localDifficulty += global + (Random.Range(1.0f, 5.0f) / 10);
    }
    
    void Start()
    {
        
    }
}
