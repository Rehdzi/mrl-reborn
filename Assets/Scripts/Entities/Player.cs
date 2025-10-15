using Entities;
using UnityEngine;

namespace Entities
{
    public class Player : MonoBehaviour
    {

        [SerializeField] public GameEntity entity;
    
        public bool isAlive = true;
        public bool isSelectable;
    
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StackManager.Instance.stack.Add(gameObject);
            
            isSelectable =  true;
        }

        private void OnDestroy()
        {
            StackManager.Instance.stack.Remove(gameObject);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (isSelectable)
            {
                gameObject.layer = LayerMask.NameToLayer("Selectable");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

}
