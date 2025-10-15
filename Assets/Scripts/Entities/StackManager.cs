using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class StackManager : MonoBehaviour
    {
        public static StackManager Instance { get; set; }

        public List<GameObject> stack = new List<GameObject>();
        public List<GameObject> selected = new List<GameObject>();
		
		private int currentSelectedIndex = -1;
		
		public GameObject CurrentSelected
		{
			get
			{
				if (selected == null || selected.Count == 0)
				{
					return null;
				}
				if (currentSelectedIndex < 0 || currentSelectedIndex >= selected.Count)
				{
					currentSelectedIndex = 0;
				}
				return selected[currentSelectedIndex];
			}
		}

		public void CycleSelected(int step = 1)
		{
			if (selected == null || selected.Count == 0)
			{
				currentSelectedIndex = -1;
				return;
			}

			if (currentSelectedIndex < 0)
			{
				currentSelectedIndex = 0;
			}

			int count = selected.Count;
			int nextIndex = (currentSelectedIndex + step) % count;
			if (nextIndex < 0)
			{
				nextIndex += count;
			}
			currentSelectedIndex = nextIndex;
		}
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
