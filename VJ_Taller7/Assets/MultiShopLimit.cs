using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MultiShopLimit : NetworkBehaviour
{
    [Header("Impulso")]
    public float impulseForce;
    public Transform whereToThrow; //Punto con el que calcular la direccion a la que salen volando
    [Header("Zona impulso")]
    public Vector3 impulseZoneSize;
    [SerializeField] LayerMask playerLayer;

    [Header("Zona Partes de la tienda")]
    public GameObject GunsExpo; //Por si las moscas necesito la referencia
    public GameObject ShopBridge;
    public GameObject ShopBarrier;
    public GameObject ShopEndTrigger;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ShopBarrier.SetActive(false);
        ShopBridge.SetActive(true);
        ShopEndTrigger.SetActive(false);
    }

    public void WaveStarted(){
        
        ShopBridge.SetActive(false);

        //SendPlayerToMap(); no funciona lindamente
        ShopEndTrigger.SetActive(true);

        StartCoroutine(WaitForBarrier());

        WaveStartedRpc();
    }
    [Rpc(SendTo.Everyone)]
    public void WaveStartedRpc()
    {
        if (IsServer) return;
        ShopBridge.SetActive(false);

        //SendPlayerToMap(); no funciona lindamente
        ShopEndTrigger.SetActive(true);

        StartCoroutine(WaitForBarrier());
    }

    private IEnumerator WaitForBarrier(){
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(3.0f);
        ShopBarrier.SetActive(true);
    }
    public void WaveFinished(bool killedAll){
        ShopBarrier.SetActive(false);
        ShopBridge.SetActive(true);
        ShopEndTrigger.SetActive(false);
        WaveFinishedRpc(killedAll);
    }

    [Rpc(SendTo.Everyone)]
    public void WaveFinishedRpc(bool killedAll)
    {
        if(IsServer) return;

        ShopBarrier.SetActive(false);
        ShopBridge.SetActive(true);
        ShopEndTrigger.SetActive(false);
    }

    [ContextMenu("Mandar a volar a los siameses")]
    private void SendPlayerToMap(){
            Collider[] playersInZone = Physics.OverlapBox(
            transform.position,
            impulseZoneSize * 0.5f, // OverlapBox usa half extents
            transform.rotation,
            playerLayer
        );

        foreach (Collider col in playersInZone)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log("Y volo, y yo vole de el (TIENDA-CHAN)");
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    ApplyFlyingForce(rb);
                }
            }
        }
    }

    private void ApplyFlyingForce(Rigidbody rbPlayer){
        Vector3 direccionFinal = (whereToThrow.position - rbPlayer.position).normalized;
        rbPlayer.AddForce(direccionFinal * impulseForce, ForceMode.VelocityChange);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar la caja de overlap
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, impulseZoneSize);
        
        // Dibujar la dirección del rebote
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.yellow;
        Vector3 direccion = (whereToThrow.position - transform.position);
        Gizmos.DrawLine(transform.position, transform.position + direccion * 2f);
        
        // Flecha indicadora
        Vector3 right = Quaternion.LookRotation(direccion) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direccion) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;
        Gizmos.DrawRay(transform.position + direccion * 2f, right * 0.5f);
        Gizmos.DrawRay(transform.position + direccion * 2f, left * 0.5f);
    }
}
