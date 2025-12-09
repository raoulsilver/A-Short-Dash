using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement2d : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed,jumpVelocity,jumpTimeMultiplier,extraGravity;

    public bool grounded;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    private int maxNumOfExtraJumps;
    private int numOfExtraJumps;
    [SerializeField]
    Transform restartTransform;
    [SerializeField]
    Material blueMaterial,redMaterial;
    float frameCount;
    bool already = false;
    float lastX = 0;

    float timerCount = 0;
    float timerMax = 0.90909091f;

    float lastX2 = 0;

    [SerializeField]
    GameManager gameManager;
    Color custBlue = new Color(0.3304557f,0.725712f,0.8867924f);
    Color custRed = new Color(0.8862745f,0.3294118f,0.4891949f);
    void Start()
    {
        GetComponent<MeshRenderer>().material = redMaterial;
        maxNumOfExtraJumps = PlayerPrefs.GetInt("Feathers");
        numOfExtraJumps = maxNumOfExtraJumps;
        rb = gameObject.GetComponent<Rigidbody2D>();
        lastX2 = transform.position.x;
    }

    void Restart()
    {
        transform.position = restartTransform.position;
        numOfExtraJumps = maxNumOfExtraJumps;
        gameManager.Reset();
        grounded = true;
    }

    void Update()
    {
        
        //Extra Jump Check
        if(Input.GetKeyDown(KeyCode.Space) && !grounded)
        {
            if(numOfExtraJumps > 0)
            {
                //Debug.Log("test");
                rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
                numOfExtraJumps -=1;
                if(numOfExtraJumps <= 0)
                {
                    //Make Player Blue here
                    //spriteRenderer.color = custBlue;
                    GetComponent<MeshRenderer>().material = blueMaterial;
                }
            }
        }
        // timer
        timerCount += Time.deltaTime;
        if (timerCount >= timerMax)
        {

            //Debug.Log(transform.position.x - lastX2);
            lastX2 = transform.position.x;
            timerCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
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
        if(Input.GetKey(KeyCode.Space) && grounded)
        {
            frameCount = 0;
            if(numOfExtraJumps < 1)
            {
                GetComponent<MeshRenderer>().material = blueMaterial;
            }
            grounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
            //rb.AddForce(transform.up* (2.8f - 0f - (Physics2D.gravity * 1f) * .4f * .4f * 0.5f)/.4f));
            if(numOfExtraJumps <= 0)
            {
                //Make Player Blue Here
            }

        }
        if (!grounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y -= extraGravity*Time.deltaTime;
            rb.linearVelocity = vel;
        }
        
       
       //Vector3 movement = new vector3(moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
       
       //transform.position.x += moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.identity;
        //transform.position = new Vector3(transform.position.x+(Time.deltaTime*speed),transform.position.y+yVelocity,transform.position.z);
        
    }

    void CheckHitWall()
    {
        Debug.Log(transform.position.x-lastX);
        if(transform.position.x-lastX <=0)
        {
            Restart();
        }
        lastX = transform.position.x;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(moveSpeed,rb.linearVelocity.y,0);
        CheckHitWall();
        
    }
    void LateUpdate()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("feather"))
        {
            numOfExtraJumps = maxNumOfExtraJumps;
            collision.gameObject.SetActive(false);
            //Make Player Red Here
            GetComponent<MeshRenderer>().material = redMaterial;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            if(normal == Vector3.up)
            {
                //Make Player Red Here
                GetComponent<MeshRenderer>().material = redMaterial;
                
                grounded = true;
                already = false;
                //Debug.Log(frameCount*60);
            }
            
        }
        if (collision.gameObject.CompareTag("spike"))
        {
            Restart();
        }
        
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            grounded = true;
        }  
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            //Debug.Log("test");
            grounded = false;
        }
    }
}
