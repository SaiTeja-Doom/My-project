using System;
using DG.Tweening;
using UnityEngine;

public class FirstPersonCam : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;
    public Transform orientation;
    public Transform camHolder;
    private float rotationX;
    private float rotationY;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update camera based on mouse movements
        float mouseX = Input.GetAxisRaw("Mouse X")*Time.deltaTime*sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y")*Time.deltaTime*sensitivityY;
        
        rotationX-=mouseY;
        rotationY+=mouseX;

        // Fix cam max movement
        rotationX = Math.Clamp(rotationX, -90f,90f);
        
        // Rotate Cam holder
        camHolder.rotation = Quaternion.Euler(rotationX,rotationY,0);
        orientation.rotation = Quaternion.Euler(0,rotationY,0);
    }

    public void DoFov(float endValue){
        GetComponent<Camera>().DOFieldOfView(endValue,0.25f);
    }

    public void DoTilt(float zTilt){
        transform.DOLocalRotate(new Vector3(0,0,zTilt),0.25f);
    }
}
