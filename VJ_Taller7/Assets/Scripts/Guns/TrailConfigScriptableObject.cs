using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Guns Trail Config", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject {
    public Material TrailMaterial;
    public AnimationCurve WidthCurve;
    public float Duration;
    public float MinVertexDistance;
    public Gradient ColorGradient;

    public float MissDistance = 100f; //en caso de no apuntar a nada, fija un punto a esta distancia
    public float SimulationSpeed = 100f; //Basicamente la velocidad que aparece el trail y la bala 
}