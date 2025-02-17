using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform player;
    public Transform Player{
        get => player;
        set => player = value;
    }

    [SerializeField] private Enemy enemy;

    [SerializeField] private float updateRate = 0.1f;
    public float UpdateRate{
        get => updateRate;
        set => updateRate = value;
    }
    
    private NavMeshAgent agent;
    private AgentLinkMover linkMover;
    private Coroutine followCoroutine;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;

    private const string IsWalking = "IsWalking";
    private const string Jump = "IsJumping";
    private const string Landed = "Landed";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;

        if(enemy != null){
            enemy = GetComponent<Enemy>();
        }
    }

    public void StartChasing()
    {
        if (enemy != null && enemy.IsStatic)
        {
            //Won't chase if static
            return;
        }

        if(followCoroutine == null){
            followCoroutine = StartCoroutine(FollowTarget());
        }
        else{
            Debug.LogWarning("Llamaste StartChasing en un enemigo que ya esta en estado chasing");
        }
    }

    private IEnumerator FollowTarget(){
        WaitForSeconds wait = new WaitForSeconds(UpdateRate);

        while (enabled){

            if(enemy.Health <= 0){
                followCoroutine = null;
                yield break;
            }

            agent.SetDestination(player.transform.position);
            yield return wait;
        }
    }

    private void HandleLinkStart(){
        animator.SetTrigger(Jump); 
    }

    private void HandleLinkEnd()
    {
        animator.SetTrigger(Landed);
    }

    private void Update()
    {
        HandleAnims();
    }

    private void HandleAnims()
    {
        if(agent.hasPath){
            Vector3 dir = (agent.steeringTarget - transform.position).normalized;
            Vector3 animDir = transform.InverseTransformDirection(dir);

            speedX.Update();
            speedY.Update();

            speedX.TargetValue = animDir.x;
            speedY.TargetValue = animDir.z;

            animator.SetFloat("Horizontal", speedX.CurrentValue);
            animator.SetFloat("Vertical", speedY.CurrentValue);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);
        }
        else{
            animator.SetFloat("Horizontal", 0, 0.25f, Time.deltaTime);
            animator.SetFloat("Vertical", 0, 0.25f, Time.deltaTime);
        }    
    }
}
