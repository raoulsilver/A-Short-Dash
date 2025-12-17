using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement2d : MonoBehaviour
{
    public bool frozen = false;
    [SerializeField]
    private float moveSpeed,jumpVelocity,jumpTimeMultiplier,extraGravity;
    [SerializeField] private CameraShake cameraShake;

    public bool grounded;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip walkClip;
    private AudioSource walkSource;
    private AudioSource jumpSource;
    [SerializeField, Range(0f,1f)] private float jumpVolume = 0.8f;
    [SerializeField, Range(0f,1f)] private float walkVolume = 0.6f;
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
    Color custBlue = new Color(0.2233593f,0.2330129f,0.3702503f);
    Color custRed = new Color(0.7025235f,0.1595969f,0.1511034f);
    [SerializeField]
    Material claireMaterial;
    void Start()
    {
        GetComponent<MeshRenderer>().material = redMaterial;
        maxNumOfExtraJumps = PlayerPrefs.GetInt("Feathers");
        numOfExtraJumps = maxNumOfExtraJumps;
        rb = gameObject.GetComponent<Rigidbody2D>();
        walkSource = gameObject.AddComponent<AudioSource>();
        walkSource.loop = true;

        jumpSource = gameObject.AddComponent<AudioSource>();
        jumpSource.loop = false;
        lastX2 = transform.position.x;
        claireMaterial.SetColor("_BaseColor",custRed);
    }

    void Restart()
    {
        if (cameraShake != null)
            StartCoroutine(cameraShake.Shake(0.8f, 0.85f));
        transform.position = restartTransform.position;
        numOfExtraJumps = maxNumOfExtraJumps;
        gameManager.Reset();
        grounded = true;
        claireMaterial.SetColor("_BaseColor",custRed);
    }

    void Update()
    {
        //Extra Jump Check
        if (numOfExtraJumps > 0)
        {
            claireMaterial.SetColor("_BaseColor",custRed);
        }
        
        if(numOfExtraJumps < 1)
            {
                claireMaterial.SetColor("_BaseColor",custBlue);
            }
        if(Input.GetKeyDown(KeyCode.Space) && !grounded && !frozen)
        {
            if(numOfExtraJumps > 0)
            {
                //Debug.Log("test");
                rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
                jumpSource.PlayOneShot(jumpClip, jumpVolume);
                if (walkSource.isPlaying)
                    walkSource.Stop();
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

        /*if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }*/
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
        if(Input.GetKey(KeyCode.Space) && grounded && !frozen)
        {
            frameCount = 0;

            grounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,jumpVelocity);
            jumpSource.PlayOneShot(jumpClip, jumpVolume);
            if (walkSource.isPlaying)
                walkSource.Stop();
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
        
        if (grounded && !walkSource.isPlaying && walkClip != null)
        {
            walkSource.clip = walkClip;
            walkSource.volume = walkVolume;
            walkSource.Play();
        }
        else if (!grounded && walkSource.isPlaying)
        {
            walkSource.Stop();
        }
       
       //Vector3 movement = new vector3(moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
       
       //transform.position.x += moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.identity;
        //transform.position = new Vector3(transform.position.x+(Time.deltaTime*speed),transform.position.y+yVelocity,transform.position.z);
        
    }

    void CheckHitWall()
    {
        //Debug.Log(transform.position.x-lastX);
        if(transform.position.x-lastX <=0 && !frozen)
        {
            Restart();
        }
        lastX = transform.position.x;
    }

    void FixedUpdate()
    {
        if(!frozen) {
            rb.linearVelocity = new Vector3(moveSpeed,rb.linearVelocity.y,0);
        }
        if (frozen)
        {
            rb.linearVelocity = new Vector3(0,0,0);
        }
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
