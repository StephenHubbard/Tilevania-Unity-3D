using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{

    // Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathKick = new Vector2(100f, 100f);

    [SerializeField] AudioClip attackSFX;
    [SerializeField] AudioClip kickSFX;
    [SerializeField] AudioClip deathSFX;

    [SerializeField] GameObject playerAttackColliderGameObject;
    [SerializeField] BoxCollider2D playerAttackCollider;

    // State
    bool isAlive = true;

    // Cached component references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider2D;
    BoxCollider2D myFeet;
    float gravityScaleAtStart;



    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider2D = GetComponent<CapsuleCollider2D>();
        myFeet = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
    }

    void Update()
    {
        if (!isAlive) { return; }

        Run();
        ClimbLadder();
        Jump();
        Attack();
        FlipSprite();
        Die();
    }

    

    private void Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // value is between -1 to +1
        Vector2 playerVelocity = new Vector2(controlThrow * runSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("Running", playerHasHorizontalSpeed);
    }

    private void ClimbLadder()
    {
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myAnimator.SetBool("Climbing", false);
            myRigidBody.gravityScale = gravityScaleAtStart;
            return;
        }

        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlThrow * climbSpeed);
        myRigidBody.velocity = climbVelocity;
        myRigidBody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("Climbing", playerHasVerticalSpeed);

    }

    private void Jump()
    {
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            return;
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            myAnimator.SetTrigger("Jump");
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd;

        }
    }

    private void Attack()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            int randomAttackInt = Random.Range(1, 3);
            myAnimator.SetTrigger("Attack" + randomAttackInt);
            if (randomAttackInt == 1)
            {
                AudioSource.PlayClipAtPoint(attackSFX, Camera.main.transform.position);
            }
            else if (randomAttackInt == 2)
            {
                AudioSource.PlayClipAtPoint(kickSFX, Camera.main.transform.position);
            }
        }
    }

    public void AttackColliderActiveTrue()
    {
        playerAttackColliderGameObject.SetActive(true);
    }

    public void AttackColliderActiveFalse()
    {
        playerAttackColliderGameObject.SetActive(false);
    }

    private void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }
    }

    private void Die()
    {
        if (myBodyCollider2D.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, .5f);
            myRigidBody.velocity = deathKick;
            myAnimator.SetTrigger("Die");
            isAlive = false;

            StartCoroutine(ProcessDeathWaitTime());

            IEnumerator ProcessDeathWaitTime()
            {
                yield return new WaitForSeconds(2f);
                FindObjectOfType<GameSession>().ProcessPlayerDeath();
            }
        }
    }
}
