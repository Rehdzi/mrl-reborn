using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Entities
{
    public class InputController
    {
        private InputActionMap actionMap = InputSystem.actions.FindActionMap("Movement");

        public void MoveEntity(Camera cam, NavMeshAgent agent, LayerMask mask)
        {
            if (actionMap.FindAction("Move").WasPerformedThisFrame())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    agent.SetDestination(hit.point);
                }
            }
        }

        public void SelectEntity(Camera cam, NavMeshAgent agent, LayerMask mask)
        {
            if (actionMap.FindAction("Select").WasPerformedThisFrame())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    
                }
            }
        }
    }
}