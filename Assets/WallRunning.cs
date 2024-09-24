using UnityEngine;

public class WallRunning : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform orientation;
    public PlayerMovement playerMovement;
    public FirstPersonCam firstPersonCam;
    public KeyCode wallJumpKey = KeyCode.Space;
    public KeyCode wallRunUpKey = KeyCode.LeftShift;
    public KeyCode wallRunDownKey = KeyCode.LeftControl;
    public float gravityCounterForce;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float exitWallTime;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    public float wallRunCheckDistance;
    public float minJumpHeight;
    private float wallRunTimer;
    private float horizontalInput;
    private float verticalInput;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool leftWall;
    private bool rightWall;
    private bool runningUp;
    private bool runningDown;
    private bool exitingWall;
    private float exitingWallTimer;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        playerMovement=GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        CheckForWallRun();
    }

    private void FixedUpdate() {
        if(playerMovement.GetIsWallRunning()){
            WallRun();
        }
    }

    private void CheckForWallRun() {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A,D
        verticalInput = Input.GetAxisRaw("Vertical"); // W,S

        runningUp = Input.GetKey(wallRunUpKey);
        runningDown = Input.GetKey(wallRunDownKey);

        if((leftWall||rightWall) && verticalInput > 0 && IsAboveGround() && !exitingWall){
            //Start wallRun
            if(!playerMovement.GetIsWallRunning()){
                StartWallRun();
            }
            wallRunTimer -= Time.deltaTime;
            if(wallRunTimer <= 0){
                exitingWall = true;
                exitingWallTimer = exitWallTime;
            }
            if(Input.GetKeyDown(wallJumpKey)){
                WallJump();
            }
        } else if (exitingWall) {
            if(playerMovement.GetIsWallRunning()){
                StopWallRun();
            }
            exitingWallTimer -= Time.deltaTime;
            if(exitingWallTimer <= 0) {
                exitingWall = false;
            }
        } else {
            // End Wall run
            StopWallRun();
        }
    }

    private void CheckForWall() {
        // Check for left and right walls
        leftWall = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallRunCheckDistance, wallLayer);
        rightWall = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallRunCheckDistance, wallLayer);
    }

    private bool IsAboveGround() {
        return !Physics.Raycast(transform.position, Vector3.down, out _, minJumpHeight, groundLayer);
    }

    private void StartWallRun(){
        playerMovement.SetIsWallRunning(true);
        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        // Change cam
        firstPersonCam.DoFov(90f);
        firstPersonCam.DoTilt(rightWall ? 5f:-5f);
    }

    private void WallRun(){
        Vector3 wallNormal = rightWall?rightWallHit.normal:leftWallHit.normal;
        Vector3 movementDirection = Vector3.Cross(wallNormal,transform.up);

        // Change direction if the direction shown is backward
        if((orientation.forward - movementDirection).magnitude > (orientation.forward - -movementDirection).magnitude){
            movementDirection = -movementDirection;
        }

        // Add force on char
        rb.AddForce(movementDirection*wallRunForce,ForceMode.Force);

        // Move char up/down
        if(runningUp){
            rb.velocity = new Vector3(rb.velocity.x,wallClimbSpeed,rb.velocity.z);
        }
        if(runningDown){
            rb.velocity = new Vector3(rb.velocity.x,-wallClimbSpeed,rb.velocity.z);
        }

        // Push char to wall if user has no horizontal Input to come out
        if(!(horizontalInput>0 && leftWall) && !(horizontalInput<0 && rightWall)){
            rb.AddForce(-wallNormal*100f,ForceMode.Force);
        }

        // Add gravity counter force
        rb.AddForce(transform.up*gravityCounterForce,ForceMode.Force);
    }
    private void StopWallRun(){
        playerMovement.SetIsWallRunning(false);
        //Turn on gravity
        rb.useGravity = true;

        // Reset Change cam
        firstPersonCam.DoFov(80f);
        firstPersonCam.DoTilt(0f);
    }

    private void WallJump() {
        // Exit Wall for some time so no wall run starts in next frame
        exitingWallTimer = exitWallTime;
        exitingWall=true;

        Vector3 wallNormal = rightWall ? rightWallHit.normal: leftWallHit.normal;

        Vector3 forceOfJump = transform.up*wallJumpUpForce + wallNormal*wallJumpSideForce;

        //Make the jump and reset y velocity to not go to space
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);
        rb.AddForce(forceOfJump,ForceMode.Impulse);
    }
}
