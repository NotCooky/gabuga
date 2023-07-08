using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    Rigidbody grabbedObj;
    public LineRenderer lr;
    public Transform camPos;
    public Transform targetPos;
    public V3PIDController piddy;
    public float ForceMultiplier;
    public float throwingForce;
    float zPos;
    float mouseX, mouseY;
    float xRotation, yRotation;
    PlayerMove playerMoveScript;

    void Start()
    {
        lr.positionCount = 2;
        playerMoveScript = PlayerMove.Instance;
    }

    void Update()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX;
        xRotation -= mouseY;


        zPos += Input.mouseScrollDelta.y / 4;
        zPos = Mathf.Clamp(zPos, 1.25f, 3);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(1) && Physics.Raycast(camPos.transform.position, camPos.transform.forward, out hit, 3f) && hit.rigidbody)
        {
            grabbedObj = hit.rigidbody;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            grabbedObj = null;
            lr.enabled = false;
            targetPos.localPosition = new Vector3(0, 0.04f, 1.5f);
        }

        if (grabbedObj != null)
        {
            lr.enabled = true;
            lr.SetPosition(0, targetPos.position);
            lr.SetPosition(1, grabbedObj.position);

            targetPos.position = camPos.position + camPos.forward * zPos;

            if (Input.GetKey(KeyCode.R))
            {
                PlayerMove.Instance.CanLook = false;
                grabbedObj.transform.rotation = Quaternion.Euler(xRotation, yRotation, grabbedObj.rotation.z);
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                PlayerMove.Instance.CanLook = true;
            }

            if (Input.GetMouseButton(0))
            {
                grabbedObj.AddForce(camPos.transform.forward * throwingForce, ForceMode.Impulse);
                grabbedObj = null;
                lr.enabled = false;
            }
        }
        else if (grabbedObj == null) PlayerMove.Instance.CanLook = true;



    }

    void FixedUpdate()
    {
        //pid stuff
        if (grabbedObj != null)
        {
            Vector3 Error = targetPos.position - grabbedObj.transform.position;
            grabbedObj.AddForce(piddy.GetOutput(Error) * ForceMultiplier);
        }
    }
}