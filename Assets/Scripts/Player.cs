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
    float attackComboTimer;
    public float timeBeforeComboReset = 1;
    public float wallCheckDistance = 0.25f;
    public float climbUpSpeed = 2.0f;
    bool isFacingRight;
    bool isHangingFromLedge;
    bool canHangFromLedge;


    private void Start()
    {
        canHangFromLedge = true;
        currentAttackNumber = 0;
        animator.SetInteger("AttackNumber",currentAttackNumber);
        rigidbody = GetComponent<Rigidbody2D>();
        airJumpsRemaining = 0;
        airMovementSpeed = walkingMovementSpeed;
    }

    private void Update()
    {
        GetInput();
        ComboResetter();
    }



    private void FixedUpdate()
    {
        if (rigidbody.velocity.y < 0 || rigidbody.velocity.y > 0)
        {
            //movement in air
            inAir = true;
            rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * airMovementSpeed * Time.fixedDeltaTime, rigidbody.velocity.y);
            animator.SetFloat("YVelocity", (rigidbody.velocity.y > 0)? 1 : -1);
        }
        else
        {
            if (inAir)
            {
                animator.SetBool("Grounded", true);
            }
            // movement on ground
            inAir = false;
            rigidbody.velocity = new Vector2(horizontal * ((Input.GetKey(KeyCode.LeftShift)) ? runningMovementSpeed : walkingMovementSpeed) * Time.fixedDeltaTime, rigidbody.velocity.y);

            animator.SetFloat("Speed" , Mathf.Abs((Input.GetKey(KeyCode.LeftShift)? horizontal : horizontal / 2)));
        }
        LedgeChecker();
    }

    void GetInput ()
    {
        if (animator.GetBool("IsAttacking"))
        {
            // is attacking so prevent input
            horizontal = 0;
            return;
        }

        if (animator.GetBool("IsSliding"))
        {
            // is sliding so prevent input
            return;
        }
        if (isHangingFromLedge)
        {
            horizontal = 0;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Drop from ledge
                isHangingFromLedge = false;
                animator.SetBool("IsHanging", false);
                rigidbody.gravityScale = 1;
                StartCoroutine("DelayLedgeGrabbing");
            }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Climb up ledge
                StartCoroutine("DelayLedgeGrabbing");
                StartCoroutine("CoClimbUp");
                isHangingFromLedge = false;

            }
            return;
        }



        // Move Left & Right Input
        horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
        {
            transform.localScale = new Vector3(-1,1,1);
            isFacingRight = false;
        }
        else if (horizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isFacingRight = true;
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
            attackComboTimer = timeBeforeComboReset;
            currentAttackNumber++;
            currentAttackNumber %= 3;
            animator.SetInteger("AttackNumber",currentAttackNumber);
        }

        // Slide Input
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (inAir == false)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    animator.SetTrigger("Slide");
                }
            }
        }
    }

    void Jump ()
    {
        // Set jumping animation and calculating velocity based on desired jump height and adding it to the rigibody
        animator.SetBool("Grounded", false);
        float yVelocity =  Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rigidbody.gravityScale));
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, yVelocity);
    }

    //Resets the combo if the player doesnt attack for x amount of time
    private void ComboResetter()
    {
        if (currentAttackNumber > 0)
        {
            if (attackComboTimer <= 0)
            {
                currentAttackNumber = 0;
                animator.SetInteger("AttackNumber", currentAttackNumber);
            }
            else
            {
                attackComboTimer -= Time.deltaTime;
            }
        }
    }


    // Raycasts to check if is next to wall, then does another raycast starting from above the player head and if it doesnt hit some thing the player is at a ledge 
    // stop gravity and veloctity and play ledge hang animation 
    void LedgeChecker ()
    {
        if (canHangFromLedge == false)
        {
            return;
        }
        if (inAir)
        {
            if (isFacingRight)
            {
                // Right
                if (Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y+ 0.8f),Vector2.right, wallCheckDistance,groundLayer))
                {
                    print("wall detected");
                    if (!Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 1.2f), Vector2.right, 1, groundLayer))
                    {
                        print("Ledge Detected");
                        rigidbody.gravityScale = 0;
                        rigidbody.velocity = Vector2.zero;
                        rigidbody.angularVelocity = 0;
                        isHangingFromLedge = true;
                        animator.SetBool("IsHanging",true);
                    }

                }
            }
            else
            {
                // Left
                if (Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 0.8f), Vector2.left, wallCheckDistance, groundLayer))
                {
                    print("wall detected");
                    if (!Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y + 1.2f),Vector2.left, 1 ,groundLayer))
                    {
                        print("Ledge Detected");
                        rigidbody.gravityScale = 0;
                        rigidbody.velocity = Vector2.zero;
                        rigidbody.angularVelocity = 0;
                        isHangingFromLedge = true;
                        animator.SetBool("IsHanging", true);
                    }
                }
            }
        }
    }

    // delay so the player can drop down from the ledge without immediately grabbing back on
    IEnumerator DelayLedgeGrabbing ()
    {
        canHangFromLedge = false;
        yield return new WaitForSeconds(0.25f);
        canHangFromLedge = true;
    }

    IEnumerator CoClimbUp ()
    {
        animator.SetBool("IsHanging", false);
        animator.Play("Sommersault");
        Vector2 firstTarget = new Vector2(transform.position.x + ((isFacingRight) ? 0.5f : -0.5f), transform.position.y + 2.5f);
        while (Vector2.Distance(transform.position, firstTarget) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, firstTarget, climbUpSpeed * Time.deltaTime);
            yield return null;
        }
        Vector2 finalTarget = new Vector2(transform.position.x + ((isFacingRight)? 0.5f: -0.5f), transform.position.y + 1.0f);
        while (Vector2.Distance(transform.position, finalTarget) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position,finalTarget,climbUpSpeed * Time.deltaTime);
            yield return null;
        }
        rigidbody.gravityScale = 1.0f;
    }
}
