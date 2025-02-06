using UnityEngine;

public class BasicBulletBehavior : MonoBehaviour
{
    [SerializeField] private int baseDamage; //Setearlo en conjunto con las stats del player-->in the future
    [SerializeField] private float lifeTime;
    private float actualLifeTime;
    private Rigidbody rb;
    private bool isShoot;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable() {
        isShoot=true;
        actualLifeTime=lifeTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<EnemyBase>() != null){
            other.gameObject.GetComponent<EnemyBase>().TakeDamage(baseDamage);
        }
        
        rb.linearVelocity =Vector3.zero;
        rb.angularVelocity =Vector3.zero;
        gameObject.SetActive(false);
    }

    private void Update() {
        if(isShoot){
            actualLifeTime-= Time.deltaTime;
        }
    }
    private void FixedUpdate() {
        if (actualLifeTime<=0){
            ProjectileEnd();
        }
    }

    public void ProjectileEnd(){
        isShoot=false;
        gameObject.SetActive(false);
        Destroy(gameObject); //mientras miro lo de la pool
    }
}
