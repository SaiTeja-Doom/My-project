using System.Collections;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    public float maxSlideTime;
    public float slideForce;
    public float slideYscale;
    public KeyCode slideKey = KeyCode.LeftControl;
    public Transform orientation;
    public Transform characterBody;
    private Rigidbody rb;
    private PlayerMovement pm;
    private float startYScale;
    private float horizontalInput;
    private float verticalInput;
    private float slideTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale=characterBody.localScale.y;
    }

    void Update()
    {

        horizontalInput = Input.GetAxisRaw("Horizontal"); // A,D key
        verticalInput = Input.GetAxisRaw("Vertical"); // W,S key
        CheckSliding();
    }


    void FixedUpdate()
    {
        if(pm.GetIsSliding()){
            SlidingMain();
        }
    }


    private void CheckSliding() {
        if(Input.GetKeyDown(slideKey) && (horizontalInput!=0||verticalInput!=0)){
            StartSlide();
        }
        if(Input.GetKeyUp(slideKey) && pm.GetIsSliding()){
            StopSLide();
        }
    }
    private void StartSlide() {
        pm.SetIsSliding(true);
        slideTime = 0f;
        transform.localScale = new Vector3(transform.localScale.x,slideYscale,transform.localScale.z);
        // Move body down so it comes down instantly
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void SlidingMain() {

        Vector3 inputDirection = GetSlideMovementDirection();

        if(!pm.OnSlope() || rb.velocity.y >=0) {
            // On ground slide normally or on slope going up
            rb.AddForce(slideForce * inputDirection,ForceMode.Force);
            slideTime += Time.deltaTime;
        } else {
            //On slopes slide forever and inn slope direction      
            rb.AddForce(slideForce * pm.GetSlopeMovementDirection(inputDirection),ForceMode.Force);
        }

        if(slideTime>=maxSlideTime){
            StopSLide();
        }
    }

    private void StopSLide() {
        pm.SetIsSliding(false);
        transform.localScale = new Vector3(transform.localScale.x,startYScale,transform.localScale.z);
    }

    private Vector3 GetSlideMovementDirection() {
        return orientation.forward*verticalInput + orientation.right*horizontalInput;
    }
}