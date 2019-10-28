using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 0.25f;
    public float jumpForce = 7;
    public float ScarfLength = 25f;
    public float GrappleSpeed = 0.1f;
    public float distanceGround;

    public LayerMask groundLayers;
    public CapsuleCollider col;
    public Image Crosshairs;
    public bool isGrounded = false;
    public bool isAgainstWallDuringGrapple = false;
    public bool jumpedGrappledAlready = false;
    public bool haveGrappled = false;

    private Rigidbody rb;
    private float rbOriginalMass;

    private float currentSpeed;
    private Vector3 heldObjectOffset;
    private bool isAiming;
    private bool readyToGrab;
    private bool inMagnesis;
    private bool haveLetGoOfMouse;
    private Color standardMoveable;
    private Color highlightedMoveable;
    private Color selectedMoveable;
    private GameObject inCrosshairs;
    private GameObject heldObject;
    private HashSet<GameObject> moveableObjects;

    private Vector3 grappleLocation;
    private bool doGrapple = false;

    private Vector3 movementVector;
    private Vector3 currentVelocity;

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

        // Search for all existing moveable objects and put them in a hash set
        GameObject[] foundMoveableObjects = GameObject.FindGameObjectsWithTag("Moveable");
        moveableObjects = new HashSet<GameObject>(foundMoveableObjects);

        Crosshairs.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        distanceGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        haveLetGoOfMouse = true;
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


        // Velocity for after un-grappling
        if (!isGrounded && haveGrappled)
        {
            rb.AddForce(transform.forward * GrappleSpeed, ForceMode.Impulse);
        }

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift)) { currentSpeed = Speed * 2; }
        else { currentSpeed = Speed; }

        // Aim 
        Crosshairs.enabled = isAiming = Input.GetMouseButton(1);
        if (isAiming)
        {
            if (!inMagnesis) { aim(); }
            if (readyToGrab && Input.GetMouseButton(0) && haveLetGoOfMouse)
            {
                Magnesis();
            }
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
            haveLetGoOfMouse = true;
        }

        // Zooms camera in to player
        if (Input.GetMouseButtonDown(1)) { Camera.main.SendMessage("StartSkill"); }

        // Zooms camera back out to orbit and clears any skill-specific stuff
        if (Input.GetMouseButtonUp(1))
        {
            foreach (GameObject mgo in moveableObjects) { mgo.GetComponent<Renderer>().material.SetColor("_Color", standardMoveable); }
            Camera.main.SendMessage("EndSkill");
            StopMagnesis();
            haveLetGoOfMouse = true;
        }

        // Make movement happen in direction the camera is facing
        movementVector = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * movementVector;
        GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * currentSpeed);
        if (movementVector != Vector3.zero && !isAiming) { transform.forward = movementVector; }
        rb.AddForce(movementVector * Speed);


        // Prevents player form jumping up a wall
        if (!Physics.Raycast (transform.position, -Vector3.up, distanceGround + 0.1f))
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
            jumpedGrappledAlready = false;
            haveGrappled = false;
        }

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !doGrapple)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Asserts whether player is against a wall during grapple or not
        if(doGrapple && Physics.Raycast(rb.transform.position, rb.transform.forward, 1.0f))
        {
            isAgainstWallDuringGrapple = true;
        }
        else
        {
            isAgainstWallDuringGrapple = false;
        }

        // Asserts whether player is against wall in general.
        // Used for velocity after grappling.
        if(Physics.Raycast(rb.transform.position, rb.transform.forward, 1.0f))
        {
            haveGrappled = false;
        }
    }

    private void aim()
    {
        // This line makes it so, while aiming, the player and camera both face in the 
        // same direction and the they move together when rotating around the y axis.
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
        if (!inMagnesis && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit aimHit, ScarfLength) && !doGrapple)
        {
            if (aimHit.collider.gameObject.tag == "Moveable" && haveLetGoOfMouse)
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
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, transform.position, 0.05f);
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        if (Input.GetKey(KeyCode.E) && distanceFromPlayer < ScarfLength)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(25f), .05f);
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

        if (Input.GetKeyDown(KeyCode.F) && distanceFromPlayer <= 5f)
        {

            ThrowObject(fromCamera.GetPoint(100f) - heldObject.transform.position);
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
        haveLetGoOfMouse = false;
        GetComponent<LineRenderer>().enabled = false;
    }

    private void ThrowObject(Vector3 direction)
    {
        GameObject toThrow = heldObject;
        StopMagnesis();
        toThrow.GetComponent<Rigidbody>().velocity = direction * 1.5f;
    }

    // Grapple
    private void Grapple()
    {
        Ray ray;

        if(!isAiming)
        {
            ray = new Ray(rb.transform.position, Camera.main.transform.forward);
        }
        
        else
        {
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        }

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
        //doGrapple = false;
        GetComponent<LineRenderer>().enabled = false;
        rb.mass = rbOriginalMass;
        rb.useGravity = true;
        //rb.freezeRotation = false;
        if (doGrapple && isAgainstWallDuringGrapple && !jumpedGrappledAlready)
        {
            rb.AddForce(Vector3.up * (jumpForce * 1.2f), ForceMode.Impulse);
            jumpedGrappledAlready = true;
        }
        doGrapple = false;

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
        haveGrappled = true;


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = Vector3.Lerp(transform.position, grappleLocation, GrappleSpeed * Time.deltaTime);
    }

    public void AddMoveableObjectToList(GameObject toAdd)
    {
        moveableObjects.Add(toAdd);
    }

    public void RemoveMoveableObjectFromList(GameObject toRemove)
    {
        moveableObjects.Remove(toRemove);
    }
}
