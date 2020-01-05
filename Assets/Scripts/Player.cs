using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public Animator animator;
    Rigidbody2D rigidbody;
    float horizontal;
    public float walkingMovementSpeed = 150.0f;
    public float runningMovementSpeed = 300.0f;
    [Header("Percentage of Running Movement Speed")]
    [Range(0.0f,1.0f)]
    public float airMovementSpeed = 0.25f;
    public float jumpHeight = 5;
    public int airJumps = 1;
    bool inAir;
    public LayerMask groundLayer;
    int airJumpsRemaining;
    int currentAttackNumber = 0;

    private void Start()
    {
        currentAttackNumber = 0;
        animator.SetInteger("AttackNumber",currentAttackNumber);
        rigidbody = GetComponent<Rigidbody2D>();
        airJumpsRemaining = 0;
        airMovementSpeed = walkingMovementSpeed;
    }

    private void Update()
    {
        GetInput();
    }


    private void FixedUpdate()
    {
        if (rigidbody.velocity.y < 0 || rigidbody.velocity.y > 0)
        {
            inAir = true;
            rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * airMovementSpeed * Time.fixedDeltaTime, rigidbody.velocity.y);
            animator.SetFloat("YVelocity", (rigidbody.velocity.y > 0)? 1 : -1);
            print((rigidbody.velocity.y > 0) ? 1 : -1);
        }
        else
        {
            if (inAir)
            {
                animator.SetBool("Grounded", true);
            }

            inAir = false;
            rigidbody.velocity = new Vector2(horizontal * ((Input.GetKey(KeyCode.LeftShift)) ? runningMovementSpeed : walkingMovementSpeed) * Time.fixedDeltaTime, rigidbody.velocity.y);
            animator.SetFloat("Speed" , Mathf.Abs((Input.GetKey(KeyCode.LeftShift)? horizontal : horizontal / 2)));
        }        
    }

    void GetInput ()
    {
        if (animator.GetBool("IsAttacking"))
        {
            horizontal = 0;
            return;
        }
        // Move Left & Right Input
        horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
        {
            transform.localScale = new Vector3(-1,1,1);
        }
        else if (horizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }


        // Jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics2D.OverlapCircle(new Vector3(transform.position.x,transform.position.y - 1f,transform.position.z),0.25f,groundLayer))
            {
                airMovementSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runningMovementSpeed : walkingMovementSpeed;
                Jump();
                airJumpsRemaining = airJumps;
            }
            else if (airJumpsRemaining > 0)
            {
                airJumpsRemaining--;
                Jump();
            }
        }

        // Slash Input
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z))
        {
            if (inAir)
            {
                animator.SetTrigger("Slash");
            }
            else
            {
                animator.SetTrigger("Slash");
            }
            currentAttackNumber++;
            currentAttackNumber %= 3;
            animator.SetInteger("AttackNumber",currentAttackNumber);
        }
    }

    void Jump ()
    {
        animator.SetBool("Grounded", false);
        float yVelocity =  Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rigidbody.gravityScale));
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, yVelocity);
    }
}
