using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float hMove;
    private float vMove;
    private bool isJumping;
    private bool grounded;
    private bool jetpackBursting;
    private bool grappling;
    private bool swinging;
    private bool wasSwinging;
    private bool m_FacingRight;
    private Collider2D[] groundCheckCircle;
    private Vector2 grapplePoint;
    private float ropeLength;
    public float jumpForce;
    public float speed;
    public float maxSpeed;
    public float burstPower;
    public float ropeRetractSpeed;
    public Rigidbody2D rb;
    public Transform groundCheckPos;
    public Camera cam;
    public LineRenderer lineRen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hMove = Input.GetAxisRaw("Horizontal");
        vMove = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }
        if (Input.GetKeyDown("left shift"))
        {
            jetpackBursting = true;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            grappling = true;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            swinging = false;
        }
    }

    private void FixedUpdate()
    {
        //Test si le personnage touche le sol
        groundCheckCircle = Physics2D.OverlapCircleAll(groundCheckPos.position,0.1f);
        grounded = false;
        for (int i = 0; i < groundCheckCircle.Length; i++)
        {
            if (groundCheckCircle[i].gameObject.layer == 3)
            {
                grounded = true;
                wasSwinging = false;
            }
            else
            {
                grounded = false;
            }
        }
        //Ne peut sauter qu'une fois qu'il touche le sol
        if (isJumping && grounded)
        {
            rb.AddForce(new Vector2(0, jumpForce));
            isJumping = false;
            grounded = false;
        }
        //Si on est à une vitesse acceptable, on garde le contrôle du personnage
        if ((Mathf.Abs(rb.velocity.x) <= maxSpeed) && !wasSwinging) 
        {
            rb.AddForce(new Vector2((hMove * maxSpeed - rb.velocity.x) * speed,0));
        }
        //Le burst de vitesse du jetpack
        if (jetpackBursting)
        {

            rb.velocity = new Vector2(hMove * burstPower, vMove * burstPower);
            jetpackBursting = false;
        }
        //Gestion du grappin
        if (grappling)
        {
            Vector2 mousePos;
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D raycast = Physics2D.Raycast(rb.position, mousePos - rb.position,Mathf.Infinity,LayerMask.GetMask("Obstacles"));
            if (raycast) {
                grapplePoint = raycast.point;
                ropeLength = Vector2.Distance(grapplePoint,rb.position);
                lineRen.SetPosition(0,new Vector3(grapplePoint.x,grapplePoint.y));
                lineRen.SetPosition(1, new Vector3(rb.position.x, rb.position.y));
                lineRen.loop = true;
                grappling = false;
                swinging = true;
            } else
            {
                grappling = false;
            }
        }
        if (vMove != 0 && swinging)
        {
            ropeLength -= vMove*ropeRetractSpeed;
        }
        if (swinging)
        {
            wasSwinging = true;
            Vector2 futurePos = rb.position + rb.velocity * Time.fixedDeltaTime;
            lineRen.SetPosition(1, new Vector3(rb.position.x, rb.position.y));
            if (Vector2.Distance(grapplePoint,futurePos) > ropeLength)
            {
                futurePos = (futurePos - grapplePoint).normalized;
                futurePos = grapplePoint + (futurePos * ropeLength);
                rb.velocity = (futurePos - rb.position)/Time.fixedDeltaTime;
            } else
            {
                ropeLength = Vector2.Distance(grapplePoint, rb.position);
            }
        } else {
            lineRen.loop = false;
            lineRen.SetPosition(0, Vector3.zero);
            lineRen.SetPosition(1, Vector3.zero);
        }
        if (hMove < 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (hMove > 0 && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
    }
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}