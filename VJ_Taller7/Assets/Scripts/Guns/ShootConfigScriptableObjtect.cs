using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigScriptableObjtect : ScriptableObject {

    [Header("GeneralStats")]
    public bool IsAutomatic;
    //public float TimeBetweenShooting; //Para las armas no automácticas, el tiempo para aceptar entre cada input
    public bool IsHitScan = true;
    public float FireRate; //tiempo entre disparos, INREVIEW para los proyectiles
    public int  BulletsPerShot = 1; //cuantos disparos se hacen por cada input
    public bool HaveSpread;
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);


    [Header("HitScanStats")]
    public LayerMask HitMask;

    [Header("ProjectileStats")]
    public GameObject BulletPrefab; //para las especiales, asignar el projectil manualmente

    public object Clone() {
        ShootConfigScriptableObjtect clone = CreateInstance<ShootConfigScriptableObjtect>();

        Utilities.CopyValues(this, clone);

        return clone;
    }
}