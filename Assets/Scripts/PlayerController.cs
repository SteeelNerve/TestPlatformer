using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // init variables
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;

    // FSM
    private enum State { idle, running, jumping, falling, hurt, climb }
    private State state = State.idle;

    // ladder
    [HideInInspector] public bool canClimb = false;
    [HideInInspector] public bool bottomLadder = false;
    [HideInInspector] public bool topLadder = false;
    public Ladder ladder;
    private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;

    // inspector variables
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float hurtForce = 10f;
    private int countdownTimePowerUp = 10;
    [SerializeField] private AudioSource cherry;
    [SerializeField] private TextMeshProUGUI countdownText;
    private int countdownTimePowerUpCache;
    private bool countdownHasStarted = false;
    //private Coroutine coroutine;

    private void Awake()
    {
        countdownText.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        countdownText.text = "";
        countdownTimePowerUpCache = countdownTimePowerUp;
        naturalGravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.climb)
        {
            Climb();
        }
        else if (state != State.hurt)
        {
            Movement();
        }

        AnimationState();
        anim.SetInteger("state", (int)state); // setup animation
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            cherry.Play();
            Destroy(collision.gameObject);
            PermanentUI.perm.cherries += 1;
            PermanentUI.perm.cherryText.text = "X " + PermanentUI.perm.cherries.ToString();
        }
        if (collision.tag == "Powerup")
        {
            cherry.Play();
            jumpForce = 17f;
            GetComponent<SpriteRenderer>().color = Color.yellow;
            Destroy(collision.gameObject);
            PowerUpStart();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (collision.gameObject.tag == "Enemy")
        {
            if (state == State.falling)
            {
                enemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                HandleHealth();
                if (collision.gameObject.transform.position.x > transform.position.x)
                {
                    // enemy is right, take damage and kick left
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    private void HandleHealth()
    {
        PermanentUI.perm.health -= 1;
        PermanentUI.perm.healthAmount.text = "Health " + PermanentUI.perm.health.ToString();
        if (PermanentUI.perm.health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Movement()
    {
        float hDirection = (Input.GetAxis("Horizontal"));

        if(canClimb && Mathf.Abs(Input.GetAxis("Vertical")) > .1f)
        {
            state = State.climb;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector3(ladder.transform.position.x, rb.position.y);
            rb.gravityScale = 0f;
        }

        // moving left
        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }
        //moving right
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }
        // jumping
        if (Input.GetButtonDown("Jump"))
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 1.3f, ground);
            if (hit.collider != null)
            {
                Jump();
            }
            /*if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
            {
                Jump();
            }*/
          
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void AnimationState()
    {   
        if(state == State.climb)
        {

        }
        else if (state == State.jumping)
        {
            if (rb.velocity.y < .1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if (state == State.hurt)
        {
            if (Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > .1f)
        {
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private void PowerUpStart()
    {
        if (!countdownHasStarted)
        {
            StartCoroutine(PowerTimer());
        }
        else
        {
            countdownTimePowerUp = countdownTimePowerUpCache + 1;
        }
    }

    private IEnumerator PowerTimer()
    {
        Debug.Log(countdownHasStarted);
        countdownHasStarted = true;
        countdownText.gameObject.SetActive(true);
        while (countdownTimePowerUp > 0)
        {
            countdownText.text = "POWER " + countdownTimePowerUp.ToString() + " s";
            yield return new WaitForSeconds(1f);

            countdownTimePowerUp--;
        }
        jumpForce = 10f;
        GetComponent<SpriteRenderer>().color = Color.white;
        countdownHasStarted = false;
        countdownText.gameObject.SetActive(false);
        countdownTimePowerUp = countdownTimePowerUpCache;
    }

    private void Climb()
    {
        float vDirection = Input.GetAxis("Vertical");
        float hDirection = Input.GetAxis("Horizontal");

  


        if (Input.GetButtonDown("Jump"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = naturalGravity;
            anim.speed = 1f;
            Jump();
            return;
        }

        // climbing up
        if(vDirection > .1f && !topLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        // climbing down
        else if (vDirection < -.1f && !bottomLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        else if (vDirection < -.1f && bottomLadder)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = naturalGravity;
            canClimb = false;
            anim.speed = 1f;
            state = State.idle;
        }
        
        else if (vDirection == 0 && hDirection == 0)
        {
            anim.speed = 0f;
            rb.velocity = Vector2.zero;
        }
        
        // do nothing
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.velocity = Vector2.zero;
        }
    }
}


