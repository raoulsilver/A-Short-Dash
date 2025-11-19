using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed,jumpVelocity,jumpTimeMultiplier,extraGravity;

    private bool grounded;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    private int maxNumOfJumps;
    private int numOfJumps;
    [SerializeField]
    Transform restartTransform;
    [SerializeField]
    Material blueMaterial,redMaterial;
    float frameCount;
    bool already = false;
    float lastX = 0;


    Color custBlue = new Color(0.3304557f,0.725712f,0.8867924f);
    Color custRed = new Color(0.8862745f,0.3294118f,0.4891949f);
    void Start()
    {
        GetComponent<MeshRenderer>().material = redMaterial;
        numOfJumps = maxNumOfJumps;
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = restartTransform.position;
            numOfJumps = maxNumOfJumps;
        }
        if (!grounded)
        {
            frameCount+= Time.deltaTime;
        }
        if(!grounded && rb.linearVelocityY <0 && already == false)
        {
            already = true;
            //Debug.Log(frameCount*60);
            frameCount = 0;
        }
        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            frameCount = 0;
            GetComponent<MeshRenderer>().material = blueMaterial;
            grounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
            //rb.AddForce(transform.up* (2.8f - 0f - (Physics2D.gravity * 1f) * .4f * .4f * 0.5f)/.4f));

        }
        if (!grounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y -= extraGravity*Time.deltaTime;
            rb.linearVelocity = vel;
        }
        else if(Input.GetKeyDown(KeyCode.Space) && !grounded)
        {
            if(numOfJumps > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
                numOfJumps -=1;
                if(numOfJumps <= 0)
                {
                    //spriteRenderer.color = custBlue;
                }
            }
        }
        rb.linearVelocity = new Vector3(moveSpeed,rb.linearVelocity.y,0);
        transform.rotation = Quaternion.identity;
        //transform.position = new Vector3(transform.position.x+(Time.deltaTime*speed),transform.position.y+yVelocity,transform.position.z);
        
    }
    void FixedUpdate()
    {

    }
    void LateUpdate()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("feather"))
        {
            numOfJumps = maxNumOfJumps;
            spriteRenderer.color = custRed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            if(normal == Vector3.up)
            {
                GetComponent<MeshRenderer>().material = redMaterial;
                grounded = true;
                already = false;
                //Debug.Log(frameCount*60);
            }
            
        }
        
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            grounded = false;
        }
    }
}
