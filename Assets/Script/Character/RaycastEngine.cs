using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastEngine : MonoBehaviour
{
    public static RaycastEngine Instance { get; private set; }
    public PlayerAbilities PlayerAbilities { get; private set; }

    /// <summary>
    /// SHoudl ONLY be modified by the class GameSceneManager
    /// </summary>
    public bool InputValid_SceneManager { get; set; }
    /// <summary>
    /// Should ONLY be modified by the class GameDialogueManager
    /// </summary>
    public bool InputValid_DialogueManager { get; set; }
    /// <summary>
    /// Should ONLY be modified by playerStaminaSystem scripts attached to this player
    /// </summary>
    public bool InputValid_StaminaManager { get; set; }
    /// <summary>
    /// Should ONLY be modified by playerInteractionSystem script attached to this player
    /// </summary>
    //public bool InputValid_InteractionManager { get; set; }

    private void Awake() {
        if (RaycastEngine.Instance != null) {
            Debug.LogWarning("2 instances of player character, deleting new one");
            Destroy(this.gameObject);
        }  else {
            RaycastEngine.Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        PlayerAbilities = new();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    enum JumpState {
        None=0, Holding
    }
    enum DropDownState {
        None = 0, Dropping = 1
    }
    enum ClutchState {
        None = 0, LeftClutch = 1, RightClutch = 2, Lockout = 3
    }
    enum DashState {
        None = 0, Dashing = 1
    }


    [SerializeField] LayerMask platformMask;
    [SerializeField] float gravity;
    [SerializeField] float groundCheckLength;
    [SerializeField] Vector2 bottomLeftEdge;
    [SerializeField] Vector2 bottomRightEdge;
    [SerializeField] Vector2 topLeftEdge;
    [SerializeField] Vector2 topRightEdge;

    [SerializeField] float parallelInsetLen;
    [SerializeField] float perpendicularInsetLen;


    [SerializeField] float horizontalUpAcceleration;
    [SerializeField] float horizontalDownAcceleration;
    [SerializeField] float horizontalDownAccelerationGrounded;
    [SerializeField] float horizontalMaxSpeed;
    [SerializeField] float horizSnapSpeed;


    [SerializeField] float jumpBuffer;
    [SerializeField] float jumpStartSpeed;
    [SerializeField] float jumpMaxPeriod;
    [SerializeField] float jumpMinSpeed;
    [SerializeField] float jumpPeakGravityBoost;
    [SerializeField] float jumpTimeMinimum;
    [SerializeField] float jumpGraceTime;
    [SerializeField] float jumpBoostFinal;
    
    [SerializeField] float dropBuffer;
    [SerializeField] float dropTime;

    [SerializeField] float dashBuffer;
    [SerializeField] float dashStartSpeed;
    [SerializeField] float dashMinimumSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashBoostFinal;

    [SerializeField] float wallClutchRange;
    [SerializeField] float wallClutchTime;
    [SerializeField] float wallClutchStillTime;
    [SerializeField] float wallClutchVerticalMaxSpeed;
    [SerializeField] float wallClutchVerticalAcceleration;
    [SerializeField] float clutchGraceTime;
    [SerializeField] float clutchExitHorizontalVelocity;
    [SerializeField] float clutchDirectionalLockoutTime;

    [SerializeField] float launchForceMultiplier;

    [SerializeField] PlayerStaminaSystem staminaSystem;

    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] AudioSource jumpSource, landSource, dashSource;

    
    float jumpHoldTimer;
    JumpState jumpState;
    bool doubleJumpAvailable;
    float jumpGraceTimestamp;

    float dropTimer;
    DropDownState dropState;
    
    float clutchTimer;
    ClutchState clutchState;
    float clutchJumpLeftLockoutTimestamp;
    float clutchJumpRightLockoutTimestamp;

    bool dashAvailable;
    DashState dashState;
    float dashTimer;

    Collider2D lastStandingOn;
    Vector2 lastStandingOnPos;
    Vector2 lastStandingOnVel;
    private Vector2 velocity;

    bool wasHoldingJump;
    bool wasGrounded;

    PlayerInputSpace inputSpace;

    float delayToIdle = 0;


    RaycastMoveDirection moveDown;
    RaycastMoveDirection moveLeft;
    RaycastMoveDirection moveRight;
    RaycastMoveDirection moveUp;
    RaycastCheckTouch groundDown;
    RaycastCheckTouch wallSideRight;
    RaycastCheckTouch wallSideLeft;

    void Start()
    {
        groundDown = new RaycastCheckTouch(bottomLeftEdge, bottomRightEdge, Vector2.down, platformMask,
            Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen, groundCheckLength);

        wallSideLeft = new RaycastCheckTouch(bottomLeftEdge, topLeftEdge, Vector2.left, platformMask,
            Vector2.up * parallelInsetLen, Vector2.right * perpendicularInsetLen, wallClutchRange);
        wallSideRight = new RaycastCheckTouch(bottomRightEdge, topRightEdge, Vector2.right, platformMask,
            Vector2.up * parallelInsetLen, Vector2.left * perpendicularInsetLen, wallClutchRange);


        moveDown = new RaycastMoveDirection(bottomLeftEdge, bottomRightEdge, Vector2.down, platformMask,
            Vector2.right * parallelInsetLen, Vector2.up * perpendicularInsetLen);
        moveLeft = new RaycastMoveDirection(bottomLeftEdge, topLeftEdge, Vector2.left, platformMask,
            Vector2.up * parallelInsetLen, Vector2.right * perpendicularInsetLen);
        moveRight = new RaycastMoveDirection(bottomRightEdge, topRightEdge, Vector2.right, platformMask,
            Vector2.up * parallelInsetLen, Vector2.left * perpendicularInsetLen);
        moveUp = new RaycastMoveDirection(topLeftEdge, topRightEdge, Vector2.up, platformMask,
            Vector2.right * parallelInsetLen, Vector2.down * perpendicularInsetLen);

        inputSpace = new();

    }


    bool InputValid() {
        return InputValid_DialogueManager && InputValid_SceneManager && InputValid_StaminaManager;// && InputValid_InteractionManager;
    }

    void Update()
    {
        if (!InputValid() || dead) { inputSpace.ResetSpace(); return; }
        if (Input.GetButtonDown("Jump")) {
            inputSpace.JumpTimestamp = Time.time + jumpBuffer;
        }
        inputSpace.JumpInputDown = Input.GetButton("Jump");

        //inputSpace.DashInputDown = 
        if (Input.GetButtonDown("Dash")) {
            Debug.Log("dash input");
            inputSpace.DashTimestamp = Time.time + dashBuffer;
        }
        
        if (Input.GetAxisRaw("Vertical") < 0) {
            inputSpace.DropTimestamp = Time.time + dropBuffer;
        }
    }


    private int GetSign(float v) {
        if (Mathf.Approximately(v, 0)) {
            return 0;
        } else if (v > 0) {
            return 1;
        } else { 
            return -1;
        }
    }





    private void FixedUpdate() {
        Collider2D standingOn = groundDown.DoRaycast(transform.position);
        bool grounded = standingOn != null;
        bool onMovingPlatform = false;


        if (!grounded && wasGrounded) {
            jumpGraceTimestamp = Time.time + jumpGraceTime;
        }
        if (grounded && GameDataManager.Instance.PlayerAbilityData.abilities[2] == true) {
            doubleJumpAvailable = true;
        }
        if (grounded && GameDataManager.Instance.PlayerAbilityData.abilities[1] == true) {
            dashAvailable = true;
        }

        int mementoIndex = 0;
        foreach (bool b in GameDataManager.Instance.PlayerAbilityData.abilities) {
            if (b) {
                mementoIndex += 1;
            }
        }
        bool finalBoost = mementoIndex > 4;


        bool clutchDirectionSwap = false;
        switch (clutchState) {
            case ClutchState.None:
                if (GameDataManager.Instance.PlayerAbilityData.abilities[0] == false) { break; }
                if (grounded) { break; }
                if (!InputValid()) { break; }
                if (velocity.y > 0.5f) { break; }
                Collider2D rightCollider = wallSideRight.DoRaycast(transform.position);
                Collider2D leftCollider = wallSideLeft.DoRaycast(transform.position);
                
                if (rightCollider != null && !rightCollider.CompareTag("Border")) { 
                    velocity = Vector2.zero;
                    velocity.x = 100.0f;
                    clutchState = ClutchState.RightClutch;
                    jumpState = JumpState.None;
                    dashState = DashState.None;
                    if (GameDataManager.Instance.PlayerAbilityData.abilities[2] == true) {
                        doubleJumpAvailable = true;
                    }
                    if (GameDataManager.Instance.PlayerAbilityData.abilities[1] == true) {
                        dashAvailable = true;
                    }
                    clutchTimer = 0;
                } else if (leftCollider != null && !leftCollider.CompareTag("Border")) {
                    velocity = Vector2.zero;
                    velocity.x = -100.0f;
                    clutchState = ClutchState.LeftClutch;
                    jumpState = JumpState.None;
                    dashState = DashState.None;
                    if (GameDataManager.Instance.PlayerAbilityData.abilities[2] == true) {
                        doubleJumpAvailable = true;
                    }
                    if (GameDataManager.Instance.PlayerAbilityData.abilities[1] == true) {
                        dashAvailable = true;
                    }
                    clutchTimer = 0;
                }
                break;
            case ClutchState.LeftClutch:
                clutchTimer += Time.deltaTime;
                if (clutchTimer > wallClutchStillTime) {
                    velocity.y = Mathf.Max(velocity.y + -wallClutchVerticalAcceleration * Time.deltaTime, -wallClutchVerticalMaxSpeed);
                }
                velocity.x = -100.0f;
                Collider2D leftColliderCheck = wallSideLeft.DoRaycast(transform.position);
                if (leftColliderCheck == null || clutchTimer >= wallClutchTime) {
                    clutchState = ClutchState.Lockout;
                    clutchDirectionSwap = true;
                }

                break;
            case ClutchState.RightClutch:
                clutchTimer += Time.deltaTime;
                velocity.x = 100.0f;
                if (clutchTimer > wallClutchStillTime) {
                    velocity.y = Mathf.Max(velocity.y + -wallClutchVerticalAcceleration * Time.deltaTime, -wallClutchVerticalMaxSpeed);
                }
                Collider2D rightColliderCheck = wallSideRight.DoRaycast(transform.position);
                if (rightColliderCheck == null || clutchTimer >= wallClutchTime) {
                    clutchState = ClutchState.Lockout;
                    clutchDirectionSwap = true;
                }
                break;
            case ClutchState.Lockout:

                break;
        }
        // slide and fall on ground
        if ((clutchState == ClutchState.LeftClutch || clutchState == ClutchState.RightClutch) && grounded) {
            clutchState = ClutchState.None;
            clutchDirectionSwap = true;
        }
        
        // you are able to clutch the next time you jump after landing
        if (clutchState == ClutchState.Lockout && grounded) {
            clutchState = ClutchState.None;
        }

        // drop case
        switch (dropState) {
            case DropDownState.None:
                if (grounded && inputSpace.DropTimestamp > Time.time && clutchState == ClutchState.None) {
                    // reset input
                    inputSpace.DropTimestamp = 0;
                    // for future frames
                    dropState = DropDownState.Dropping;
                    dropTimer = 0;
                }
                break;
            case DropDownState.Dropping:
                dropTimer += Time.deltaTime;
                if (dropTimer >= dropTime) {
                    dropState = DropDownState.None;
                }
                break;
        }

        // jump case
        switch (jumpState) {
            case JumpState.None:
                if ((grounded || jumpGraceTimestamp > Time.time) 
                    && inputSpace.JumpTimestamp > Time.time 
                    && dropState == DropDownState.None
                    && clutchState == ClutchState.None) 
                    {
                    // anim
                    jumpTrigger = true;
                    //animator.SetTrigger("Jump");
                    // reset input
                    inputSpace.JumpTimestamp = 0;
                    jumpGraceTimestamp = 0;
                    // for future frames
                    jumpState = JumpState.Holding;
                    dashState = DashState.None;
                    jumpHoldTimer = 0;
                    velocity.y = jumpStartSpeed;
                    staminaSystem.JumpCast();
                } 
                else if (!grounded && doubleJumpAvailable && inputSpace.JumpTimestamp > Time.time && dropState == DropDownState.None && (clutchState == ClutchState.Lockout || clutchState == ClutchState.None)) {
                    Debug.Log("DOUBLE JUMP");
                    // anim
                    doubleJumpTrigger = true;
                    //animator.SetTrigger("Jump");
                    // reset input
                    inputSpace.JumpTimestamp = 0;
                    // for future frames
                    jumpState = JumpState.Holding;
                    dashState = DashState.None;
                    jumpHoldTimer = 0;

                    velocity.y = jumpStartSpeed;
                    if (finalBoost) { velocity.y += jumpBoostFinal; }

                    staminaSystem.SecondJumpCast();

                    doubleJumpAvailable = false;
                    
                }
                else if (clutchState == ClutchState.RightClutch && inputSpace.JumpTimestamp > Time.time) {
                    Debug.Log("JUMPING OFF WALL");
                    // anim
                    jumpTrigger = true;
                    //animator.SetTrigger("Jump");
                    // reset input
                    inputSpace.JumpTimestamp = 0;
                    // for future frames
                    jumpState = JumpState.Holding;
                    jumpHoldTimer = 0;
                    velocity.y = jumpStartSpeed;
                    staminaSystem.JumpCast();
                    // clutch exit
                    velocity.x = -clutchExitHorizontalVelocity;
                    clutchDirectionSwap = true;
                    clutchState = ClutchState.Lockout;
                    clutchJumpRightLockoutTimestamp = Time.time + clutchDirectionalLockoutTime;

                    dashState = DashState.None;
                }
                else if (clutchState == ClutchState.LeftClutch && inputSpace.JumpTimestamp > Time.time) {
                    Debug.Log("JUMPING OFF WALL");
                    // anim
                    jumpTrigger = true;
                    //animator.SetTrigger("Jump");
                    // reset input
                    inputSpace.JumpTimestamp = 0;
                    // for future frames
                    jumpState = JumpState.Holding;
                    jumpHoldTimer = 0;
                    velocity.y = jumpStartSpeed;
                    staminaSystem.JumpCast();
                    // clutch exit
                    velocity.x = clutchExitHorizontalVelocity;
                    clutchDirectionSwap = true;
                    clutchState = ClutchState.Lockout;
                    clutchJumpLeftLockoutTimestamp = Time.time + clutchDirectionalLockoutTime;

                    dashState = DashState.None;
                }
                break;
            case JumpState.Holding:
                jumpHoldTimer += Time.deltaTime;
                if ((inputSpace.JumpInputDown == false || jumpHoldTimer >= jumpMaxPeriod) && jumpHoldTimer > jumpTimeMinimum) {
                    jumpState = JumpState.None;
                    if (velocity.y < jumpStartSpeed) {
                        velocity.y = Mathf.Lerp(jumpMinSpeed, jumpStartSpeed, jumpHoldTimer / jumpMaxPeriod);
                    }
                }
                break;
        }

        // dash case
        switch (dashState) {
            case DashState.None:
                if (dashAvailable && inputSpace.DashTimestamp > Time.time) {
                    // anim
                    dashTrigger = true;
                    //animator.SetTrigger("Roll");                    //  jumpSource.Stop();
                    //  jumpSource.time = 0.2f;
                    //  jumpSource.Play();
                    // reset input
                    inputSpace.DashTimestamp = 0;
                    // for future frames
                    dashState = DashState.Dashing;
                    dashTimer = 0;
                    velocity.y = 0;
                    velocity.x = dashStartSpeed * (spriteRenderer.flipX == true ? -1 : 1);
                    if (finalBoost) { velocity.x += dashBoostFinal; }
                    dashAvailable = false;
                    staminaSystem.DashCast();
                    jumpState = JumpState.None;
                    if (clutchState == ClutchState.LeftClutch || clutchState == ClutchState.RightClutch) {
                        clutchDirectionSwap = true;
                        clutchState = ClutchState.Lockout;
                        clutchJumpLeftLockoutTimestamp = Time.time + clutchDirectionalLockoutTime;
                        velocity *= -1;
                    }
                }
                break;
            case DashState.Dashing:
                if (velocity.x == 0) {
                    dashState = DashState.None;
                }
                float direction = (spriteRenderer.flipX == true ? -1 : 1);
                velocity.x = Mathf.Max(dashMinimumSpeed, ((dashTime - dashTimer) / dashTime) * dashStartSpeed) * direction;
                velocity.y = 0;
                dashTimer += Time.deltaTime;
                if (dashTimer > dashTime) {
                    dashState = DashState.None;
                }
                
                break;
        }


        #region User Horizontal Input
        float horizInput = 0;
        if ((clutchState == ClutchState.None || clutchState == ClutchState.Lockout) && dashState == DashState.None && InputValid()) {
            horizInput = Input.GetAxisRaw("Horizontal");
            if (clutchJumpRightLockoutTimestamp > Time.time) {
                horizInput = Mathf.Min(0, horizInput);
            }
            if (clutchJumpLeftLockoutTimestamp > Time.time) {
                horizInput = Mathf.Max(0, horizInput);
            }

        } else if (clutchState == ClutchState.LeftClutch) {
            float _clutchDropHorInput = Input.GetAxisRaw("Horizontal");
            if (_clutchDropHorInput > 0) {
                clutchState = ClutchState.Lockout;
                clutchDirectionSwap = true;
            }
        } else if (clutchState == ClutchState.RightClutch) {
            float _clutchDropHorInput = Input.GetAxisRaw("Horizontal");
            if (_clutchDropHorInput < 0) {
                clutchState = ClutchState.Lockout;
                clutchDirectionSwap = true;
            }
        }
        int wantedDirection = GetSign(horizInput);
        int velocityDirection = GetSign(velocity.x);
        if (wantedDirection != 0) {
            if (wantedDirection != velocityDirection) {
                velocity.x = horizSnapSpeed * wantedDirection;
            } 
            else {
                if (Mathf.Abs(velocity.x) < horizontalMaxSpeed) {
                    velocity.x = Mathf.MoveTowards(velocity.x, horizontalMaxSpeed * wantedDirection, horizontalUpAcceleration * Time.deltaTime);
                }
            }
        } 
        else {
            /*if ((grounded && Mathf.Approximately(velocity.y, 0))) {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, horizontalDownAccelerationGrounded * Time.deltaTime);
            }
            else {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, horizontalDownAcceleration * Time.deltaTime);
            }*/
            if (grounded) {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, horizontalDownAccelerationGrounded * Time.deltaTime);
            }
            else {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, horizontalDownAcceleration * Time.deltaTime);
            }

        }
        #endregion

        Vector2 movingPlatformVel = Vector2.zero;
        if (grounded) {
            if (lastStandingOn == null || lastStandingOn != standingOn) { 
                onMovingPlatform = standingOn.GetComponent<PlatformMover>() != null;
            } else {
                if (lastStandingOn == standingOn) {
                    movingPlatformVel = (Vector2)standingOn.transform.position - lastStandingOnPos;
                    onMovingPlatform = movingPlatformVel.magnitude > 0f;
                }
            }
            
        }
        #region Gravity
        bool gravityOnGround = moveDown.DoRaycast(transform.position, groundCheckLength, dropState == DropDownState.Dropping) < 0.001f;
        if (jumpState == JumpState.None && dashState == DashState.None && (clutchState == ClutchState.None || clutchState == ClutchState.Lockout)) {
            if (grounded) {//&& !gravityOnGround) {
                if (onMovingPlatform) {
                    if (movingPlatformVel.y > 0.0f) {
                        if (!gravityOnGround) {
                            velocity.y -= gravity * Time.deltaTime;
                        }
                        else {
                            velocity.y = 0;
                        }
                    } else  {
                        Debug.Log("Giga gravity");
                        velocity.y = -20.0f;
                    }
                } else if (velocity.y > 0){
                    velocity.y -= gravity * Time.deltaTime;
                } else if (!gravityOnGround) {
                    velocity.y -= gravity * Time.deltaTime;
                }

            } 
            else if ((!grounded ||  (dropState == DropDownState.Dropping)) && dashState == DashState.None) {
                velocity.y -= gravity * Time.deltaTime;
            }
        }

        if (wasHoldingJump && jumpState != JumpState.Holding && (clutchState == ClutchState.None || clutchState == ClutchState.Lockout)) {
            // the longer you hold the jump button the less pronbounced the effect
            velocity.y -= gravity * Time.deltaTime * jumpPeakGravityBoost * ((jumpMaxPeriod - jumpHoldTimer) / jumpMaxPeriod);
        }
        #endregion

        Vector2 displacement = Vector2.zero;
        Vector2 wantedDisplacement = velocity * Time.deltaTime;

        #region Moving Platform Velocity Inheritance
        if (grounded) {
            if (lastStandingOn == standingOn) {
                lastStandingOnVel = (Vector2)standingOn.transform.position - lastStandingOnPos;
                wantedDisplacement += lastStandingOnVel;
            }
            lastStandingOnPos = standingOn.transform.position;
        } else {
            Debug.Log("NOT GROUNDED");

            if (standingOn == null && lastStandingOn != null) {
                //wantedDisplacement += lastStandingOnVel * launchForceMultiplier;
                velocity += lastStandingOnVel * launchForceMultiplier;
            }
        }
        lastStandingOn = standingOn;
        #endregion

        if (wantedDisplacement.x > 0) {
            float rightDisp = moveRight.DoRaycast(transform.position, wantedDisplacement.x, dropState == DropDownState.Dropping);
            float leftDisp = moveLeft.DoRaycast(transform.position, -2.0f, dropState == DropDownState.Dropping);

            if (!(rightDisp < 0 && leftDisp < 0)) {
                displacement.x = moveRight.DoRaycast(transform.position, wantedDisplacement.x, dropState == DropDownState.Dropping);
            }
        } 
        else if (wantedDisplacement.x < 0) {
            Debug.Log("DUCK");
            float rightDisp = moveRight.DoRaycast(transform.position, 2.0f, dropState == DropDownState.Dropping);
            float leftDisp = moveLeft.DoRaycast(transform.position, -wantedDisplacement.x, dropState == DropDownState.Dropping);
            if (!(rightDisp < 0 && leftDisp < 0)) {
                displacement.x = -moveLeft.DoRaycast(transform.position, -wantedDisplacement.x, dropState == DropDownState.Dropping);
                Debug.Log("DUCK2");
            }
        } else {
            float rightDisp = moveRight.DoRaycast(transform.position, wantedDisplacement.x, dropState == DropDownState.Dropping);
            float leftDisp = moveLeft.DoRaycast(transform.position, -wantedDisplacement.x, dropState == DropDownState.Dropping);
            if (rightDisp < 0 && leftDisp < 0) {
                // do nothing
            } else if (rightDisp < 0) {
                displacement.x = rightDisp;
            } else if (leftDisp < 0) {
                displacement.x = -leftDisp;
            }
        }

        if (wantedDisplacement.y > 0) {
            displacement.y = moveUp.DoRaycast(transform.position, wantedDisplacement.y, dropState == DropDownState.Dropping);
        } 
        else if (wantedDisplacement.y < 0) {
            displacement.y = -moveDown.DoRaycast(transform.position, -wantedDisplacement.y, dropState == DropDownState.Dropping);
        }

        if (Mathf.Approximately(wantedDisplacement.x, displacement.x) == false) {
            velocity.x = 0;
        }
        if (Mathf.Approximately(wantedDisplacement.y, displacement.y) == false) {
            velocity.y = 0;
        }

        if (disappear) {
            jumpTrigger = false;
            doubleJumpTrigger = false;
            dashTrigger = false;
            return;
        } 
        else if (dead) {
            transform.Translate(displacement);
            jumpTrigger = false;
            doubleJumpTrigger = false;
            dashTrigger = false;
            return;
            
        }
        

        if (jumpTrigger) {
            animator.SetTrigger("Jump");
            //sound
            AudioSystem.Instance.PlayJump();
            GameDataManager.Instance.jumpCount += 1;
            jumpTrigger = false;
        } 

        if (doubleJumpTrigger) {
            animator.SetTrigger("Jump");
            //sound
            AudioSystem.Instance.PlayJump(); // maybe different sound
            GameDataManager.Instance.jumpCount += 1;
            doubleJumpTrigger = false;
        }

        if (dashTrigger) {
            animator.SetTrigger("Roll");
            //sound
            AudioSystem.Instance.PlayDash();
            GameDataManager.Instance.dashCount += 1;
            dashTrigger = false;
        }
        animator.SetBool("Grounded", grounded);
        animator.SetBool("OnMovingPlatform", onMovingPlatform);
        animator.SetBool("JumpLocked", jumpHoldTimer < jumpTimeMinimum);
        animator.SetFloat("AirSpeedY", Mathf.Approximately(displacement.y, 0) ? 0 : displacement.y);
        if (Mathf.Abs(wantedDirection) > 0) {
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        } 
        else {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle <= 0)
                animator.SetInteger("AnimState", 0);
        }

        if (clutchState == ClutchState.LeftClutch) {
            spriteRenderer.flipX = true;
            animator.SetBool("WallSlide", true);
        }
        else if (clutchState == ClutchState.RightClutch) {
            spriteRenderer.flipX = false;
            animator.SetBool("WallSlide", true);
        }
        else if (clutchDirectionSwap == true) {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            animator.SetBool("WallSlide", false);

        }
        else {
            switch (GetSign(wantedDirection)) {
                case 0:
                    break;
                case 1:
                    spriteRenderer.flipX = false;
                    break;
                case -1:
                    spriteRenderer.flipX = true;
                    break;
            }
            animator.SetBool("WallSlide", false);
        }


        //Debug.Log($"displacing by {  displacement.x } and {displacement.y}");
        transform.Translate(displacement);



        wasHoldingJump = jumpState == JumpState.Holding;
        wasGrounded = grounded;
    }


    public void SetRendererXFlip(bool val) {
        spriteRenderer.flipX = val;
    }
    public bool GetRendererXFlip() {
        return spriteRenderer.flipX;
    }


    bool dead = false;
    bool disappear = false;
    bool love = false;
    bool dashTrigger = false;
    bool jumpTrigger = false;
    bool doubleJumpTrigger = false;
    public void Respawn() {
        dead = false;
        disappear = false;
        love = false;
        animator.SetTrigger("Respawn");
        staminaSystem.RefillStamina();
    }
    public void Reappear(int staminaPenalty) {
        dead = false;
        disappear = false;
        animator.SetTrigger("Respawn");
        staminaSystem.ReduceStamina(staminaPenalty);
    }
    public void Death() {
        dead = true;
        this.velocity = Vector2.zero;
        dashState = DashState.None;
        jumpState = JumpState.None;
        clutchState = ClutchState.None;
        if (animator.GetBool("WallSlide") == true) {
            SetRendererXFlip(!spriteRenderer.flipX);
            animator.SetBool("WallSlide", false);
        }
        animator.SetTrigger("Death");
    }

    public void Disappear() {
        disappear = true;
        this.velocity = Vector2.zero;
        dashState = DashState.None;
        jumpState = JumpState.None;
        clutchState = ClutchState.None;
        if (animator.GetBool("WallSlide") == true) {
            SetRendererXFlip(!spriteRenderer.flipX);
            animator.SetBool("WallSlide", false);
        }
        animator.SetTrigger("Death");
    }
    public void Love() {
        this.velocity = Vector2.zero;
        dashState = DashState.None;
        jumpState = JumpState.None;
        clutchState = ClutchState.None;
        if (animator.GetBool("WallSlide") == true) {
            SetRendererXFlip(!spriteRenderer.flipX);
            animator.SetBool("WallSlide", false);
        }
        animator.SetTrigger("Love");
    }
}



public class PlayerInputSpace {

    public float JumpTimestamp { get; set; }
    public bool JumpInputDown { get; set; }

    public float DashTimestamp { get; set; }

    public float DropTimestamp { get; set; }
    public void ResetSpace() {
        JumpTimestamp = 0;
        JumpInputDown = false;
        DropTimestamp = 0;
        DashTimestamp = 0;
    }
}



public class PlayerAbilities {
    bool _wallCling;
    public bool WallCling { get { return _wallCling; } set { _wallCling = value; } }
    bool _doubleJump;
    public bool DoubleJump { get { return _doubleJump; } set { _doubleJump = value; } }
    bool _waterWalk;
    public bool WaterWalk { get { return _waterWalk; } set { _waterWalk = value; } }
    bool _dash;
    public bool Dash { get { return _dash; } set { _dash = value; } }
    bool _platformCreator;
    public bool PlatformCreator { get { return _platformCreator; } set { _platformCreator = value; } }

    public PlayerAbilities() {
        _wallCling = false;
        _doubleJump = false;
        _waterWalk = false;
        _dash = false;
        _platformCreator = false;
    }

}