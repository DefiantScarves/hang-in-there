using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 0.25f;
    public float jumpForce = 7;
    public float ScarfLength = 100f;

    public LayerMask groundLayers;
    public CapsuleCollider col;
    public Image Crosshairs;

    private Rigidbody rb;

    private float currentSpeed;
    private Vector3 heldObjectOffset;
    private bool isAiming;
    private bool readyToGrab;
    private bool inMagnesis;
    private Color standardMoveable;
    private Color highlightedMoveable;
    private Color selectedMoveable;
    private GameObject inCrosshairs;
    private GameObject heldObject;
    private GameObject[] moveableObjects;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentSpeed = Speed;
        isAiming = false;
        standardMoveable = new Color(0f, 0f, 0f);
        highlightedMoveable = new Color(.93f, .15f, 1f);
        selectedMoveable = new Color(1f, .89f, .255f);
        moveableObjects = GameObject.FindGameObjectsWithTag("Moveable");
        Crosshairs.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement from WASD
        Vector3 movementVector = Vector3.zero;
        movementVector.x = Input.GetAxis("Horizontal");
        movementVector.z = Input.GetAxis("Vertical");

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift)) { currentSpeed = Speed * 2; }
        else { currentSpeed = Speed; }

        // Aim 
        Crosshairs.enabled = isAiming = Input.GetMouseButton(1);
        if (isAiming)
        {
            if (!inMagnesis) { aim();}
            if (readyToGrab && Input.GetMouseButton(0)) { Magnesis(); }
        }
        if (Input.GetMouseButtonUp(0)) { StopMagnesis(); }

        // Zooms camera in to player
        if (Input.GetMouseButtonDown(1)) { Camera.main.SendMessage("StartSkill"); }

        // Zooms camera back out to orbit and clears any skill-specific stuff
        if (Input.GetMouseButtonUp(1))
        {
            foreach (GameObject mgo in moveableObjects) { mgo.GetComponent<Renderer>().material.SetColor("_Color", standardMoveable); }
            Camera.main.SendMessage("EndSkill");
        }

        // Make movement happen in direction the camera is facing
        movementVector = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * movementVector;
        GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * currentSpeed);
        if (movementVector != Vector3.zero && !isAiming) { transform.forward = movementVector; }
        rb.AddForce(movementVector * Speed);

        // Jump
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x,
            col.bounds.min.y, col.bounds.center.z), col.radius * .9f, groundLayers);
    }

    private void aim()
    {
        // Rotate character with camera
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        // Paint moveable objects that are visible
        foreach (GameObject mgo in moveableObjects)
        {
            Ray toGo = new Ray(transform.position, mgo.transform.position - transform.position);
            if (!inMagnesis && Physics.Raycast(toGo, out RaycastHit hitInfo)) {
                if (hitInfo.collider.gameObject.tag == "Moveable")
                {
                    mgo.GetComponent<Renderer>().material.SetColor("_Color", highlightedMoveable);
                }
                else
                {
                    mgo.GetComponent<Renderer>().material.SetColor("_Color", standardMoveable);
                }
            }
        }

        // Paint moveable object being aimed at
        if (!inMagnesis && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit aimHit, ScarfLength))
        {
            if (aimHit.collider.gameObject.tag == "Moveable")
            {
                inCrosshairs = aimHit.collider.gameObject;
                aimHit.collider.GetComponent<Renderer>().material.SetColor("_Color", selectedMoveable);
                readyToGrab = true;
            }
            // If object aimed at is not moveable
            else
            {
                readyToGrab = false;
            }
        }
        // If no object is aimed at
        else
        {
            readyToGrab = false;
        }
    }

    private void Magnesis()
    {
        inMagnesis = true;
        if (heldObject == null)
        {
            heldObject = inCrosshairs;
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        Ray fromCamera = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        float distanceFromCamera = Vector3.Distance(Camera.main.transform.position, Camera.main.transform.position + heldObjectOffset);
        float distanceFromPlayer = Vector3.Distance(transform.position, Camera.main.transform.position + heldObjectOffset);
        heldObject.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(distanceFromCamera), 0.3f));
        //heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(distanceFromCamera), 0.3f);

        if (Input.GetKey(KeyCode.Q) && distanceFromPlayer > 5f)
        {
            //heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, transform.position, 0.025f);
            heldObject.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(heldObject.transform.position, transform.position, 0.025f));
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        if (Input.GetKey(KeyCode.E) && distanceFromPlayer < 25f)
        {
            //heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(25f), 0.025f);
            heldObject.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(25f), 0.025f));
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopMagnesis();
        }
    }

    private void StopMagnesis()
    {
        readyToGrab = false;
        inMagnesis = false;
        heldObject = null;
        inCrosshairs = null;
    }
}
