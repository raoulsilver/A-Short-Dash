using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed,jumpVelocity,fallMultiplier,lowJumpMultiplier;

    private bool grounded;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    private int maxNumOfJumps;
    private int numOfJumps;
    [SerializeField]
    Transform restartTransform;

    Color custBlue = new Color(0.3304557f,0.725712f,0.8867924f);
    Color custRed = new Color(0.8862745f,0.3294118f,0.4891949f);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = custRed;
        numOfJumps = maxNumOfJumps;
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = restartTransform.position;
            spriteRenderer.color = custRed;
            numOfJumps = maxNumOfJumps;
        }
        if(rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier-1) * Time.deltaTime;
        }
        else if(rb.linearVelocity.y > 0)
        {
            //rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier-1) * Time.deltaTime;
        }
        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            grounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && !grounded)
        {
            if(numOfJumps > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
                numOfJumps -=1;
                if(numOfJumps <= 0)
                {
                    spriteRenderer.color = custBlue;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            if(normal == Vector3.up)
            {
                grounded = true;
            }
            
        }
        if (collision.gameObject.CompareTag("feather"))
        {
            numOfJumps = maxNumOfJumps;
            spriteRenderer.color = custRed;
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
