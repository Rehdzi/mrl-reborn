using TMPro;
using UnityEngine;

public class DebugUIController : MonoBehaviour
{
    
    public TextMeshProUGUI levelText;
    public GameObject session;
    
    private int _lastLevel = int.MinValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var manager = session != null ? session.GetComponent<SessionManager>() : null;
        if (manager != null)
        {
            _lastLevel = manager.level;
            levelText.text = "Level: " + _lastLevel;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var manager = session != null ? session.GetComponent<SessionManager>() : null;
        if (manager == null) return;

        var currentLevel = manager.level;
        if (currentLevel != _lastLevel)
        {
            _lastLevel = currentLevel;
            levelText.text = "Level: " + _lastLevel;
        }
    }
}
