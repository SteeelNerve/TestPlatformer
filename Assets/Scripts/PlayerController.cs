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
    private enum State { idle, running, jumping, falling, hurt }
    private State state = State.idle;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (state != State.hurt)
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
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void AnimationState()
    {
        if (state == State.jumping)
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
}


