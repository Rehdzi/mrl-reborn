using Entities;
using UnityEngine;

namespace Generation
{
    [CreateAssetMenu(fileName = "NewBiome", menuName = "Game/Biome")]
    public class BiomeData : ScriptableObject
    {
        public string biomeName;
        public float temperature;
        public float humidity;
        
        public Entity[] entities;
    }
}