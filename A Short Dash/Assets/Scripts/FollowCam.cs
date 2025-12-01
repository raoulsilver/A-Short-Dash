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

    public float verticalDeadZone = 10f;
    private float camFollowY;

    private float basePlayerY;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        basePlayerY = target.position.y;
        camFollowY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;


        pos.x = Mathf.Lerp(pos.x, target.position.x + offsetX, Time.deltaTime * smoothX);

        float yDelta = target.position.y - camFollowY;
        if (Mathf.Abs(yDelta) > verticalDeadZone)
        {
            camFollowY = target.position.y - Mathf.Sign(yDelta) * verticalDeadZone;
        }
        // gradual recentering toward player's Y even inside dead zone
        camFollowY = Mathf.Lerp(camFollowY, target.position.y, Time.deltaTime * 0.2f);
        pos.y = Mathf.Lerp(pos.y, camFollowY + offsetY, Time.deltaTime * smoothY);

        transform.position = pos;


        float heightDelta = target.position.y - basePlayerY;
        // More subtle angle change
        heightDelta = Mathf.Clamp01(heightDelta / 3f); // reacts more strongly to height
        float targetAngle = Mathf.Lerp(groundAngle, airAngle * 0.7f, heightDelta); // lower total tilt

        Quaternion desiredRot = Quaternion.Euler(targetAngle, transform.rotation.eulerAngles.y, 0f);

        // Keep player centered while tilting
        transform.position = new Vector3(
            target.position.x + offsetX,
            transform.position.y,
            transform.position.z
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRot,
            Time.deltaTime * tiltSmooth
        );
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