using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class RagdollEnabler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool startRagdoll = false;

    private Rigidbody[] rigidbodies;
    private CharacterJoint[] joints;

    private void Awake()
    {
        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
    }

    private void Start()
    {
        if (startRagdoll)
        {
            EnableRagdoll();
        }
        else{
            EnableAnimator();
        }
    }

    public void EnableRagdoll(){
        animator.enabled = false;
        agent.enabled = false;

        foreach(CharacterJoint joint in joints){
            joint.enableCollision = true;
        }
        foreach(Rigidbody rb in rigidbodies){
            rb.linearVelocity = Vector3.zero;
            rb.detectCollisions = true;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    public void DisableAllRigidbodies(){
        foreach(Rigidbody rb in rigidbodies){
            rb.detectCollisions = false;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void EnableAnimator(){
        animator.enabled = true;
        agent.enabled = true;

        foreach(CharacterJoint joint in joints){
            joint.enableCollision = false;
        }
        foreach(Rigidbody rb in rigidbodies){
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }
}
