using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PatrolState : StateMachineBehaviour
{
    private Pokemon _pokemon;
    private NavMeshAgent _agent;
    private float _timer;
    private float _minPatrolTime = 5;
    private float _maxPatrolTime = 13;
    private float _patrolTime;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _pokemon = animator.gameObject.GetComponentInParent<Pokemon>();

        if (!_pokemon.isServer) return;

        _timer = 0;
        _patrolTime = Random.Range(_minPatrolTime, _maxPatrolTime);
        _agent = animator.gameObject.GetComponentInParent<NavMeshAgent>();
        SetRandomDestinationInZone();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_pokemon.isServer) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
            SetRandomDestinationInZone();

        _timer += Time.deltaTime;

        if (_timer > _patrolTime)
            animator.SetBool("isPatrolling", false);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_pokemon.isServer) return;

        _agent.SetDestination(_agent.transform.position);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that processes and affects root motion
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that sets up animation IK (inverse kinematics)
    }

    private void SetRandomDestinationInZone()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(_pokemon.zone.bounds.min.x, _pokemon.zone.bounds.max.x),
            Random.Range(_pokemon.zone.bounds.min.y, _pokemon.zone.bounds.max.y),
            Random.Range(_pokemon.zone.bounds.min.z, _pokemon.zone.bounds.max.z)
        );
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 50.0f, NavMesh.AllAreas)) {
            _agent.SetDestination(hit.position);
        }
    }
}
