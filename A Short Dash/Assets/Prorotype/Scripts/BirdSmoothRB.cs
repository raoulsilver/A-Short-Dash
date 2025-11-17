using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdSmoothRB : MonoBehaviour
{
    [Header("movement")]
    public float moveSpeed = 10f;

    [Header("jumping")]
    public float jumpSpeed = 12f;
    public int maxFeathers = 2;
    private int feathers;

    [Header("glide")]
    public float normalGravity = -30f;
    public float glideFallSpeed = -3f;

    private Rigidbody rb;
    private FeatherUI featherUI;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        feathers = maxFeathers;
        featherUI = FindObjectOfType<FeatherUI>();
        if (featherUI != null)
        {
            featherUI.UpdateFeathers(feathers, maxFeathers);
        }
    }

    void Update()
    {
        rb.linearVelocity = new Vector3(moveSpeed, rb.linearVelocity.y, 0f);

        if (Input.GetKeyDown(KeyCode.Space) && feathers > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpSpeed, 0f);
            feathers--;

            if (featherUI != null)
                featherUI.UpdateFeathers(feathers, maxFeathers);
        }

        if (transform.position.y < -2f)
        {
            SceneManager.LoadScene("Raoul Gliding Prototype");
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < 0)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, glideFallSpeed, 0f);
        }
        else
        {
            rb.useGravity = true;
            rb.AddForce(Vector3.up * normalGravity, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            feathers = maxFeathers;

            if (featherUI != null)
                featherUI.UpdateFeathers(feathers, maxFeathers);
        }
    }
}