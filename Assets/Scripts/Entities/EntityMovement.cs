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

        public bool isCommandedToMove;
        
        private InputActionMap actionMap;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            actionMap = InputSystem.actions.FindActionMap("Movement");
            
            cam = Camera.main;
            agent = GetComponent<NavMeshAgent>();
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
                    isCommandedToMove = true;
                    agent.SetDestination(hit.point);
                }
            }

            if (agent.hasPath == false || agent.remainingDistance <= agent.stoppingDistance)
            {
                isCommandedToMove = false;
            }
        }
    }
}


