using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControlVR : MonoBehaviour
{
    //Speed Variables// 
    public float moveSpeed;
    public float sprintSpeed;
    public float fallSpeed;
    private float currentMoveSpeed;

    //Real Height// 
    public  float playerHeightCM = 180;

    //Refs// 
    public Collider myCameraCol;
    public GameObject myCamera;
    public GameObject forwardFacing;

    public GameObject leftHand;
    public GameObject rightHand;

    //Awake// 
    private void Awake()
    {
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
    }

    //Return Values// 
    public float PlayerHeight()
    {
        /*
         ||VR Scaling|| 
            + 1ft IRL = 1/3 Unity Units
            + Unity Cube = 1x1x1 Unity Units 
            + 1CM = 0.0328084 Feet
         */

        //Eye Height (20cm) Converted to feet then divided by 3 to get IRL height// 
        return ((playerHeightCM - 20) * 0.0328084f) / 3;
    }

    //Main Functions// 
    public void ControllerMovement()
    {
        //Forward Facing Transform// 
        forwardFacing.transform.rotation = Quaternion.Euler(new Vector3(0, myCamera.transform.rotation.eulerAngles.y, 0));
        forwardFacing.transform.position = new Vector3(myCamera.transform.position.x, 0, myCamera.transform.position.z);

        //Sprinting// 
        if (Input.GetAxis("LeftPalm") > 0.1) 
        { 
            currentMoveSpeed = sprintSpeed * (1 + Input.GetAxis("LeftPalm")); 
        }
        else { currentMoveSpeed = moveSpeed; }

        //Movement// 
        Vector3 moveDir = forwardFacing.transform.forward * Input.GetAxisRaw("LeftStickVertical") * currentMoveSpeed * Time.deltaTime;
        moveDir += forwardFacing.transform.right * Input.GetAxisRaw("LeftStickHorizontal") * currentMoveSpeed * Time.deltaTime;

        //Fall//
        RaycastHit hit;
        if (Physics.Raycast(myCamera.transform.position, Vector3.down * 1000f, out hit))
        {
            if (hit.distance > PlayerHeight() + 0.1f)
            {
                moveDir.y = -fallSpeed * Time.deltaTime;
            }
        }

        //Final Movement// 
        transform.Translate(moveDir * currentMoveSpeed);
    }
    public void HandTracking()
    {
        //Raw Hand Positioning.
        leftHand.transform.position = transform.position + InputTracking.GetLocalPosition(XRNode.LeftHand);
        leftHand.transform.rotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
        rightHand.transform.position = transform.position + InputTracking.GetLocalPosition(XRNode.RightHand);
        rightHand.transform.rotation = InputTracking.GetLocalRotation(XRNode.RightHand);
    }
    public void Recenter()
    {
        //NOTE : Must be standing for this to work. Standing gives a RL position, which it can set to.// 
        if (Input.GetButtonDown("LeftStickPress"))
        {
            float cameraToGround = gameObject.transform.position.y + myCamera.transform.localPosition.y;
            float offsetY = PlayerHeight() - cameraToGround;

            gameObject.transform.Translate(0f, offsetY, 0f);
        }
    }

    //Update// 
    private void Update()
    {
        ControllerMovement();
        HandTracking();
        Recenter(); 
    }

}
