using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;

    public void SetTargetTransform(Transform targetTransform)
    { this.targetTransform = targetTransform; }

    // Update is called once per frame
    void LateUpdate()
    {
        if (targetTransform == null) return;

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }
}
