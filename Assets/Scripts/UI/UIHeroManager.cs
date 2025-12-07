using Entities;
using UnityEngine;
using System.Collections;

namespace UI
{
    public class UIHeroManager : MonoBehaviour
    {
        [SerializeField] public GameObject heroCardPrefab;
        
        public static UIHeroManager instance;
        
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
            // Используем корутину для задержки, чтобы убедиться, что StackManager инициализирован
            StartCoroutine(InitializeHeroCards());
        }
        
        private IEnumerator InitializeHeroCards()
        {
            // Ждем один кадр, чтобы убедиться, что все Awake и Start методы выполнены
            yield return null;
            
            // Проверяем наличие необходимых компонентов
            if (heroCardPrefab == null)
            {
                Debug.LogError("UIHeroManager: heroCardPrefab не назначен!");
                yield break;
            }
            
            if (StackManager.Instance == null)
            {
                Debug.LogError("UIHeroManager: StackManager.Instance не найден!");
                yield break;
            }
            
            if (StackManager.Instance.stack == null)
            {
                Debug.LogError("UIHeroManager: StackManager.Instance.stack равен null!");
                yield break;
            }
            
            // Создаем по экземпляру карточки для каждого героя в пачке
            foreach (GameObject hero in StackManager.Instance.stack)
            {
                if (hero != null)
                {
                    GameObject cardInstance = Instantiate(heroCardPrefab, transform);
                    
                    // Если у карточки есть компонент для передачи данных героя, можно добавить его здесь
                    // Например: cardInstance.GetComponent<HeroCardUI>().SetHero(hero);
                }
            }
            
            Debug.Log($"UIHeroManager: Создано {StackManager.Instance.stack.Count} карточек героев");
        }
        
        // Публичный метод для обновления карточек (можно вызвать извне при изменении стека)
        public void RefreshHeroCards()
        {
            // Удаляем старые карточки
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            // Создаем новые карточки
            StartCoroutine(InitializeHeroCards());
        }
    }
}