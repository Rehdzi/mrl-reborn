using UnityEngine;

namespace Entities
{
    public class Entity : ScriptableObject
    {
        public string name;
        public string description;
        public Sprite icon;
        public EntityType entityType;
            
    }

    public class EntityType
    {
        public string name;
        public bool isAgressive;
        public bool isRecruitable;
    }
}