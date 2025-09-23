using System.Collections;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private float _defaultTime = 0.7f;
    
    public GameObject levelPrefab;

    public static SessionManager instance { get; set; }
    
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
        StartCoroutine(GenerateLevel(_defaultTime));
    }

    IEnumerator GenerateLevel(float time)
    {
        yield return new WaitForSeconds(time);
        levelPrefab.GetComponent<Level>().GenerateMap();

    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
