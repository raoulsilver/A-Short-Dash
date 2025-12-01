using UnityEngine;

public class PenguinAnimator : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public PlayerMovement2d playerMovement;

    void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x)); // running
        anim.SetBool("isGrounded", playerMovement.grounded);  // grounded
        //anim.SetFloat("verticalVelocity", rb.linearVelocity.y); // jump vs glide
    }
}