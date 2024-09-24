using System.Collections;
using StarterAssets;
using UnityEngine;

public class ParkourScript : MonoBehaviour
{

    public float wallRunForce = 10f;
    public float wallRunDuration = 0.5f;

    public float wallRunCheckDistance = 1f; // Distance to check for wall proximity

    public float climbSpeed = 5f;

    public float climbCheckDistance = 1f;

    public float gravityDuringWallRun = 0;

    public float angleThreshold = 15.0f; // Allowable angle deviation for wall running and climbing

    public Transform Spine;

    private bool _isClimbing;

    private ThirdPersonController _controller;
    private CharacterController _charController;
    private bool _isWallRunning;

    private Rigidbody rb;

    void Start()
    {
        _controller = GetComponent<ThirdPersonController>();
        _charController = GetComponent<CharacterController>();
         rb=GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckForWallRun();
        CheckForClimb();
    }

    private void CheckForWallRun()
    {
        if(Input.GetKey(KeyCode.LeftShift) && !_isWallRunning && !_isClimbing){
            // Check for left and right walls
            bool leftWall = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftWallHit, wallRunCheckDistance);
            bool rightWall = Physics.Raycast(transform.position, transform.right, out RaycastHit rightWallHit, wallRunCheckDistance);

            if ((leftWall && leftWallHit.collider.CompareTag("Wall")) || (rightWall && rightWallHit.collider.CompareTag("Wall")))
            {
                // Check the angle of the wall
                Vector3 wallNormal = leftWall ? leftWallHit.normal : rightWallHit.normal;
                float wallAngle = Vector3.Angle(transform.forward, -wallNormal);

                Debug.Log("wallAngle:" + wallAngle);

                if (90f-angleThreshold <= wallAngle && wallAngle <= 90f+angleThreshold)
                {
                    _isWallRunning = true;
                    StartCoroutine(WallRun(leftWall ? leftWallHit : rightWallHit));
                }
            }
        }
    }

    private void CheckForClimb()
    {
        if (Input.GetKey(KeyCode.Space) && !_isWallRunning && !_isClimbing) {
            // Check for climbable object directly in front
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit climbHit, climbCheckDistance))
            {
                if(climbHit.collider.CompareTag("Wall")){
                    // Check the angle of the object
                    float climbAngle = Vector3.Angle(transform.forward, -climbHit.normal);
                    if (climbAngle <= angleThreshold)
                    {
                        _isClimbing = true;
                        StartCoroutine(Climb(climbHit));
                    }
                }
            }
        }
    }


    // Perform wall running action
    IEnumerator WallRun(RaycastHit wallHit)
    {
        Debug.Log("I am wall Running");
        float time = 0;
        Vector3 wallNormal = wallHit.normal;
        Debug.Log("wallNormal:" + wallNormal);
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;

        if (Vector3.Dot(wallForward, transform.forward) < 0) // adjust for running in the correct direction
        {
            wallForward = -wallForward;
        }

        // Set the character's rotation to face along wall
        Quaternion wallRotation = Quaternion.LookRotation(wallForward, wallNormal);
        transform.rotation = wallRotation;

        float elapsedTime = 0.0f;
        float requiredTime =0.01f;
        

        while (time < wallRunDuration)
        {
            time += Time.deltaTime;
            if(elapsedTime<requiredTime){
                elapsedTime+=Time.deltaTime;
                continue;
            }
            // Check if the player is still in contact with the wall
            if(Physics.Raycast(Spine.position, Spine.TransformDirection(-wallNormal+Spine.right), out RaycastHit hit, wallRunCheckDistance + 2f)) {
                if (hit.collider.gameObject == wallHit.collider.gameObject)
                {
                    // Apply movement along the wall
                    Vector3 moveDirection = wallForward * _controller.SprintSpeed;
                    moveDirection += Physics.gravity.y * Time.deltaTime * Vector3.up; // Apply gravity in opposite direction
                    _charController.Move(moveDirection * Time.deltaTime);
                } else {
                    break;
                }
            }

            if (Physics.Raycast(transform.position, -wallNormal, out _, wallRunCheckDistance+2f))
            {
                
            }
            yield return null;
        }

        _isWallRunning = false;
    }

    IEnumerator Climb(RaycastHit climbHit)
    {

         // Calculate the direction to move in front of the wall
         Vector3 moveDirection = climbHit.normal + transform.up; // Move slightly away from the wall and upward
         moveDirection.Normalize();
        // Move the character controller to the starting position for climbing
        _charController.Move(0.2f * Time.deltaTime * moveDirection); // Adjust moveDistance as needed
         float elapsedTime = 0.0f;
         float requiredTime =0.01f;
        while (_isClimbing)
        {
            if(elapsedTime<requiredTime){
                elapsedTime += Time.deltaTime;
                continue;
            }
            // Now apply the upward movement
            Vector3 climbDirection = climbSpeed * elapsedTime * Vector3.up;
            _charController.Move(climbDirection); // Climb upwards

            // Exit climbing if the player is no longer holding the forward key or reaches the top
            if (!_charController.isGrounded)
            {
                _isClimbing = false;
            }
            yield return null;
        }
    }
}
