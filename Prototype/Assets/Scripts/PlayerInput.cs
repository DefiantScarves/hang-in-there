using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 0.25f;
    public float jumpForce = 7;
    public float ScarfLength = 100f;
    public float GrappleSpeed = 0.1f;

    public LayerMask groundLayers;
    public CapsuleCollider col;
    public Image Crosshairs;

    private Rigidbody rb;
    private float rbOriginalMass;

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

    private Vector3 grappleLocation;
    private bool doGrapple = false;

    private Vector3 movementVector;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbOriginalMass = rb.mass;
        col = GetComponent<CapsuleCollider>();
        GetComponent<LineRenderer>().enabled = false; // Hide "Scarf"
        currentSpeed = Speed;
        isAiming = false;
        standardMoveable = new Color(0f, 0f, 0f);
        highlightedMoveable = new Color(.93f, .15f, 1f);
        selectedMoveable = new Color(1f, .89f, .255f);
        moveableObjects = GameObject.FindGameObjectsWithTag("Moveable");
        Crosshairs.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {   
        movementVector = Vector3.zero;
        

        // Get movement from WASD
        if (!doGrapple)
        {
            movementVector.x = Input.GetAxis("Horizontal");
            movementVector.z = Input.GetAxis("Vertical");
        }


        // Sprint
        if (Input.GetKey(KeyCode.LeftShift)) { currentSpeed = Speed * 2; }
        else { currentSpeed = Speed; }

        // Aim 
        Crosshairs.enabled = isAiming = Input.GetMouseButton(1);
        if (isAiming)
        {
            if (!inMagnesis) { aim(); }
            if (readyToGrab && Input.GetMouseButton(0)) { Magnesis(); }
            else if (!readyToGrab && Input.GetMouseButtonDown(0))
            {
                Grapple();
            }

            if (doGrapple)
            {
                GrappleMovePlayer();
            }
        }

        if (!readyToGrab && Input.GetMouseButtonDown(0))
        {
            Grapple();
        }

        if (doGrapple)
        {
            GrappleMovePlayer();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
            StopMagnesis();
            if (heldObject != null)
            {
                heldObject.GetComponent<Rigidbody>().freezeRotation = false;
            }
        }

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
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space) && !doGrapple)
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
            if (!inMagnesis && Physics.Raycast(toGo, out RaycastHit hitInfo))
            {
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
        GetComponent<LineRenderer>().enabled = true; // Show "Scarf"
        if (heldObject == null)
        {
            heldObject = inCrosshairs;
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        heldObject.GetComponent<Rigidbody>().freezeRotation = true;
        heldObject.GetComponent<Rigidbody>().useGravity = false;

        Ray fromCamera = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        float distanceFromCamera = Vector3.Distance(Camera.main.transform.position, Camera.main.transform.position + heldObjectOffset);
        float distanceFromPlayer = Vector3.Distance(transform.position, Camera.main.transform.position + heldObjectOffset);
        heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(distanceFromCamera), 0.3f);

        if (Input.GetKey(KeyCode.Q) && distanceFromPlayer > 5f)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, transform.position, 0.025f);
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        if (Input.GetKey(KeyCode.E) && distanceFromPlayer < 25f)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(25f), 0.025f);
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            heldObject.transform.Rotate(1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.X))
        {
            heldObject.transform.Rotate(0f, 1f, 0f);
        }
        if (Input.GetKey(KeyCode.C))
        {
            heldObject.transform.Rotate(0f, 0f, 1f);
        }

        // Draw "Scarf" line
        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        GetComponent<LineRenderer>().SetPosition(1, heldObject.transform.position);
        if (Input.GetMouseButtonUp(0))
        {
            StopMagnesis();
        }
    }

    private void StopMagnesis()
    {
        if (heldObject != null)
        {
            heldObject.GetComponent<Rigidbody>().freezeRotation = false;
            heldObject.GetComponent<Rigidbody>().useGravity = true;
        }
        readyToGrab = false;
        inMagnesis = false;
        heldObject = null;
        inCrosshairs = null;
        GetComponent<LineRenderer>().enabled = false;
    }

    // Grapple
    private void Grapple()
    {
        Ray ray = new Ray(rb.transform.position, Camera.main.transform.forward);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, ScarfLength))
        {
            if (hit.collider.CompareTag("Untagged"))
            {
                grappleLocation = hit.point;
                doGrapple = true;
            }
        }
    }

    // Stop Grappling
    private void StopGrapple()
    {
        doGrapple = false;
        GetComponent<LineRenderer>().enabled = false;
        rb.mass = rbOriginalMass;
        rb.useGravity = true;
        //rb.freezeRotation = false;
    }

    // Moves the player towards grappled location.
    private void GrappleMovePlayer()
    {
        GetComponent<LineRenderer>().enabled = true; // Show "Scarf"
        // Draw "Scarf" line
        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        GetComponent<LineRenderer>().SetPosition(1, grappleLocation);

        rb.mass = 0.1f;
        rb.useGravity = false;
        rb.freezeRotation = true;


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = Vector3.Lerp(transform.position, grappleLocation, GrappleSpeed * Time.deltaTime);
    }
}
