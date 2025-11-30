using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;

    public float smoothX = 5f;
    public float smoothY = 5f;

    public float offsetX = 5f;
    public float offsetY = 0f;

    public float groundAngle = 30f; 
    public float airAngle = 60f;  // makes it look down when you are higher up
    public float tiltSmooth = 5f;

    private float basePlayerY;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        basePlayerY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;


        pos.x = Mathf.Lerp(pos.x, target.position.x + offsetX, Time.deltaTime * smoothX);


        pos.y = Mathf.Lerp(pos.y, target.position.y + offsetY, Time.deltaTime * smoothY);

        transform.position = pos;


        float heightDelta = target.position.y - basePlayerY;
        heightDelta = Mathf.Clamp01(heightDelta / 5f);  

        float targetAngle = Mathf.Lerp(groundAngle, airAngle, heightDelta);

        Quaternion desiredRot = Quaternion.Euler(targetAngle, transform.rotation.eulerAngles.y, 0f);

        /*transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRot,
            Time.deltaTime * tiltSmooth
        );*/
    }

    public void Reset()
    {
        transform.position = new Vector3(
            target.position.x + offsetX,
            target.position.y + offsetY,
            transform.position.z
        );
    }
}