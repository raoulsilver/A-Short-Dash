using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public float smooth = 5f;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, target.position.x + 5f, Time.deltaTime * smooth);
        transform.position = pos;
    }
    public void Reset()
    {
        transform.position = new Vector3(target.position.x,transform.position.y,transform.position.z);
    }
}