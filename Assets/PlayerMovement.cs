using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    [Header("Slope Movement")]
    public float maxSlopeAngle;
    [Header("Sliding")]
    public float slideSpeed;
    [Header("Wall Running")]
    public float wallRunSpeed;
    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    public float playerHeight;
    public LayerMask groundLayer;
    public Transform orientation;
    public Transform characterBody;
    private Rigidbody rb;
    private bool grounded;
    private bool readyToJump;
    private float moveSpeed;
    private float startYScale;
    private bool isJumping;
    private bool isCroutching;
    private bool isSliding;
    private RaycastHit slopeHit;
    private float horizontalInput;
    private float verticalInput;
    private float desiredMoveSpeed;
    private Vector3 moveDirection;
    private PlayerMovementState playerMovementState;
    private float lastDesiredMoveSpeed;
    private bool isWallRunning;

    private enum PlayerMovementState {
        Sprinting,
        Walking,
        Crouching,
        InAir,
        Sliding,
        WallRunning
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        playerMovementState = PlayerMovementState.Walking;
        moveSpeed = walkSpeed;
        startYScale=characterBody.localScale.y;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, groundLayer); 

        SetPlayerMovementState();
        CheckJump();
        CheckCrouch();
    }

    public void SetIsSliding(bool isSliding){
        this.isSliding = isSliding;
    }

    public bool GetIsSliding(){
        return isSliding;
    }

    public void SetIsWallRunning(bool isWallRunning){
        this.isWallRunning = isWallRunning;
    }

    public bool GetIsWallRunning(){
        return isWallRunning;
    }

    void SetPlayerMovementState(){
        if(isWallRunning){
            playerMovementState = PlayerMovementState.WallRunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        if(isSliding){
            playerMovementState = PlayerMovementState.Sliding;
            // On slope coming down sliding
            if(OnSlope() && rb.velocity.y <0.1f){
                desiredMoveSpeed = slideSpeed;
            }
        }
        else if(grounded){
            if(Input.GetKey(sprintKey)){
                playerMovementState = PlayerMovementState.Sprinting;
                desiredMoveSpeed = sprintSpeed;
            } else if(Input.GetKey(crouchKey)) {
                playerMovementState = PlayerMovementState.Crouching;
                desiredMoveSpeed = crouchSpeed;
            }
            else {
                playerMovementState = PlayerMovementState.Walking;
                desiredMoveSpeed=walkSpeed;
            }
        } else {
            playerMovementState = PlayerMovementState.InAir;
        }

        // If desiredMoveSpeed changes drastically, change the moveSpeed slowly
        if(Mathf.Abs(desiredMoveSpeed-lastDesiredMoveSpeed) > 4f && moveSpeed !=0f){
            StopCoroutine(SmoothlyChangeToRequiredSpeed());
            StartCoroutine(SmoothlyChangeToRequiredSpeed());
        } else {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

    }

    private IEnumerator SmoothlyChangeToRequiredSpeed() {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed-moveSpeed);
        float startValue = moveSpeed;

        while (time < difference) {
            moveSpeed = Mathf.Lerp(startValue,desiredMoveSpeed,time/difference);
            time += Time.deltaTime;
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
    }

    void FixedUpdate()
    {
        PlayerMovementMain();
        SpeedControl();
    }

    private void PlayerMovementMain()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        bool onSlope = OnSlope();
        if(onSlope){
            // on a slope
            rb.AddForce(10f * moveSpeed * GetSlopeMovementDirection(moveDirection),ForceMode.Force);
            // Add a downward force to replicate gravity only if there is y velocity
            if(rb.velocity.y>0){
                rb.AddForce(10f*Vector3.down,ForceMode.Force);
            }
        }
        else if(grounded) {
            // on ground
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);  
        }
        else {
            // in air
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
        
        if(!isWallRunning){
            // Turn off gravity to make sure player won't fall down
            rb.useGravity = !onSlope;
        }
    }

    private void CheckJump()
    {
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        isJumping = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        isJumping = false;
    }

    private void CheckCrouch() {
        if(Input.GetKey(crouchKey)&& !isCroutching){
            readyToJump = false;
            transform.localScale = new Vector3(transform.localScale.x,crouchYScale,transform.localScale.z);
            // Move body down so it comes down instantly
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            isCroutching = true;
        }
        if(Input.GetKeyUp(crouchKey)){
            readyToJump = true;
            isCroutching = false;
            transform.localScale = new Vector3(transform.localScale.x,startYScale,transform.localScale.z);
        }
    }

    public bool OnSlope(){
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight*0.5f+0.3f)){
            float angle = Vector3.Angle(Vector3.up,slopeHit.normal);
            if(angle > 0f && angle <= maxSlopeAngle){
                return true;
            }
        }
        return false;
    }

    public Vector3 GetSlopeMovementDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction,slopeHit.normal).normalized;
    }

    private void SpeedControl()
    {
        if(OnSlope() && !isJumping){
            // On slopes check y velocity as well or too fast
            if(rb.velocity.magnitude > moveSpeed){
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        } else {
            // On ground Just limit horizontal velocity to have more fun
            Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        
    }
}