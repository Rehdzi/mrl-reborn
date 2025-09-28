using TMPro;
using UnityEngine;

namespace Util
{
    public class DebugUIController : MonoBehaviour
    {
        
        [Header("Level Panel")]
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI globalDiffText;
        public TextMeshProUGUI localDiffText;
        
        int lastLevel = int.MinValue;
        float lastGlobalDiff = float.MinValue;
        float lastLocalDiff = float.MinValue;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
            if (SessionManager.instance != null)
            {
                lastLevel = SessionManager.instance.level;
                levelText.text += lastLevel;

                lastGlobalDiff = SessionManager.instance.globalDifficulty;
                globalDiffText.text += lastGlobalDiff;
                
                lastLocalDiff = SessionManager.instance.localDifficulty;
                localDiffText.text += lastLocalDiff;

                //UIUtilFunctions.AssignNewValue(levelText, _lastLevel,  manager.level);
            }
        }

        // Update is called once per frame
        void Update()
        {

            //var manager = session != null ? session.GetComponent<SessionManager>() : null;
            var currentLevel = SessionManager.instance.level;
            var currentGlobalDiff = SessionManager.instance.globalDifficulty;
            var currentLocalDiff = SessionManager.instance.localDifficulty;
            if (currentLevel != lastLevel)
            {
                lastLevel = currentLevel;
                levelText.text = "Level: " + lastLevel;
                
                lastGlobalDiff =  currentGlobalDiff;
                globalDiffText.text = "Global Difficulty: " + lastGlobalDiff;
                
                lastLocalDiff =  currentLocalDiff;
                localDiffText.text = "Local Difficulty: " + lastLocalDiff;
            }
            
        }
    }

    public class UIUtilFunctions
    {
        public static void AssignNewValue(TextMeshProUGUI text, float managerValue, float cachedValue, bool isInt = true)
        {
            var newValue = managerValue;
            
            if (newValue != cachedValue)
            {
                cachedValue = newValue;
                text.text += cachedValue;
            }
        }
    }
}


