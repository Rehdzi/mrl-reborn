using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "NewHero", menuName = "Game/Hero")]
    public class Hero : GameEntity
    {
        EntityType type = EntityType.Hero;
    }
}