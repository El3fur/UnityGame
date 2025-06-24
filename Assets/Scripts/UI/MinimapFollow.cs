using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 100, 0);

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}
