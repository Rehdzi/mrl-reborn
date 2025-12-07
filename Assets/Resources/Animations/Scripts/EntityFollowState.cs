using Entities;
using UnityEngine;
using UnityEngine.AI;

public class EntityFollowState : StateMachineBehaviour
{
    private AttackController attackController;
    
    NavMeshAgent agent;
    public float attackDistance;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController = animator.transform.GetComponent<AttackController>();
        agent = animator.transform.GetComponent<NavMeshAgent>();
        attackDistance = 1.0f;      //TODO:  Grab it from Hero's stats.
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController.targetToAttack == null)
        {
            animator.SetBool("isFollow", false);
        }
        else
        {
            if (animator.transform.GetComponent<EntityMovement>().isCommandedToMove == false)
            {
                agent.SetDestination(attackController.targetToAttack.position);
                animator.transform.LookAt(attackController.targetToAttack);
        
                float distanceFromTarget = Vector3.Distance(attackController.targetToAttack.position, animator.transform.position);
                if (distanceFromTarget < attackDistance)
                {
                    agent.SetDestination(animator.transform.position);
                    animator.SetBool("IsAttack", true);
                }
            }
        }
    }

}
