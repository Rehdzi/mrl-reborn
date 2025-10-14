using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "NewEntity", menuName = "Game/GameEntity")]
    public class GameEntity : ScriptableObject
    {
        public string entityName;
        public string entityDescription;
        public Sprite entityIcon;
        public EntityType entityType;

        public int HP;
        public int MP;

        public int XP;
        public int level;

        public int speed;
        public int attackSpeed;

        public int armor;

        public enum EntityType
        {
            Hero,
            NPC,
            Skeleton,
            Bat,
            Undead,
            Dwarf,
            Bug,
            Mole,
        }
    }
    
}