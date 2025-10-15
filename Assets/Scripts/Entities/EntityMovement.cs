using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Entities
{
    public class EntityMovement : MonoBehaviour
    {
        Camera cam;
        NavMeshAgent agent;
        public LayerMask ground;
        public LayerMask selectable;
        
        private InputActionMap actionMap;
        private List<GameObject> selectedUnits;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            actionMap = InputSystem.actions.FindActionMap("Movement");
            
            cam = Camera.main;
            agent = GetComponent<NavMeshAgent>();
            selectedUnits = StackManager.Instance.stack;
        }

        // Update is called once per frame
        void Update()
        {
            if (actionMap.FindAction("Move").WasPerformedThisFrame())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
                {
                    agent.SetDestination(hit.point);
                }
            }
            
            if (actionMap.FindAction("Select").WasPerformedThisFrame())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectable))
                {
                    SelectByClick(hit.collider.gameObject);
                }
            }

            if (actionMap.FindAction("Switch").WasPerformedThisFrame())
            {
				StackManager.Instance?.CycleSelected(1);
            }
        }
        
        void SelectByClick(GameObject go)
        {
            DeselectAll();
            
            selectedUnits.Add(go);
        }
        
        void DeselectAll()
        {
            selectedUnits.Clear();
        }
    }
}


