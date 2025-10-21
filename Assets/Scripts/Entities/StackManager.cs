using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities
{
    public class StackManager : MonoBehaviour
    {
        public static StackManager Instance { get; set; }

        public List<GameObject> stack = new List<GameObject>();
        public List<GameObject> selected = new List<GameObject>();
		
        public GameObject groundMarker;
        
        public LayerMask ground;
		public LayerMask selectable;
		private InputActionMap actionMap;
		Camera cam;
        
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
	        cam = Camera.main;
	        actionMap = InputSystem.actions.FindActionMap("Movement");
        }

        // Update is called once per frame
        void Update()
        {
	        if (actionMap.FindAction("Select").WasPerformedThisFrame())
	        {
		        RaycastHit hit;
		        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		        if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectable))
		        {
			        if (actionMap.FindAction("Modifier").IsPressed())
			        {
				        MultiSelect(hit.collider.gameObject);
			        }
			        
			        SelectByClick(hit.collider.gameObject);
		        }
		        else
		        {
			        DeselectAll();
		        }
	        }

	        if (actionMap.FindAction("Switch").WasPerformedThisFrame())
	        {
		        Debug.Log("Switch");
		        throw new NotImplementedException();
	        }
	        
	        if (actionMap.FindAction("Move").WasPerformedThisFrame() && selected.Count > 0)
	        {
		        RaycastHit hit;
		        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
		        {
			        groundMarker.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
			        
			        groundMarker.SetActive(false);
			        groundMarker.SetActive(true);
		        }
	        }
        }

        private void MultiSelect(GameObject go)
        {
	        if (!selected.Contains(go))
	        {
		        selected.Add(go);
		        EnableEntityMovement(go, true);
	        }
	        else
	        {
		        EnableEntityMovement(go, false);
		        selected.Remove(go);
	        }
        }

        void SelectByClick(GameObject go)
        {
	        DeselectAll();
            
	        selected.Add(go);
	        
	        EnableEntityMovement(go, true);
        }

        void EnableEntityMovement(GameObject go, bool enable)
        {
	        go.GetComponent<EntityMovement>().enabled = enable;
        }
        
        void DeselectAll()
        {
	        foreach (GameObject go in selected)
	        {
		        EnableEntityMovement(go, false);
	        }
	        
	        groundMarker.SetActive(false);
	        
	        selected.Clear();
        }
    }
}
