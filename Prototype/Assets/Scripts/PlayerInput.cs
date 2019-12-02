using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 0.25f;
    public float jumpForce = 120f;
    public float ScarfLength = 25f;
    public float GrappleSpeed = 0.05f;
    public float distanceGround;
    public float ThrowSpeed = 1.5f;

    public int PlayerHealth = 3;

    public Material HighlightedMoveable;
    public Material SelectedMoveable;
    public Material SelectedGrapple;
    public LayerMask groundLayers;
    public CapsuleCollider col;
    public Image Crosshairs;
    public Transform StowPoint;
    public bool isGrounded = false;
    public bool isAgainstWallDuringGrapple = false;
    public bool jumpedGrappledAlready = false;
    public bool haveGrappled = false;
    public bool moveForwardMomentum = false;
    public bool movementOccuredDuringMomentum = false;

    private Rigidbody rb;
    private float rbOriginalMass;

    private float currentSpeed;
    private Vector3 heldObjectOffset;
    private bool isAiming;
    private bool readyToGrab;
    private bool inMagnesis;
    private bool haveLetGoOfMouse;
    private bool stowedObject;
    private GameObject inCrosshairs;
    private GameObject heldObject;
    private Dictionary<GameObject, Material> tkObjects;

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
        //GetComponent<LineRenderer>().enabled = false; // Hide "Scarf"
        currentSpeed = Speed;
        isAiming = false;

        // Search for all existing moveable and grapple objects and put them in a dictionary
        GameObject[] foundMoveableObjects = GameObject.FindGameObjectsWithTag("Moveable");
        GameObject[] foundGrappleObjects = GameObject.FindGameObjectsWithTag("Grapple");
        tkObjects = new Dictionary<GameObject, Material>();

        foreach (GameObject MO in foundMoveableObjects)
        {
            tkObjects.Add(MO, MO.GetComponent<MeshRenderer>().material);
        }
        foreach (GameObject GO in foundGrappleObjects)
        {
            tkObjects.Add(GO, GO.GetComponent<MeshRenderer>().material);
        }

        Crosshairs.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        distanceGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        haveLetGoOfMouse = true;
        GrappleSpeed = 0.05f;
        stowedObject = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 15.9f)
        {
            transform.position = new Vector3(-100f, 39.4f, 168f);
            //PlayerHealth--;
            reduceHealth();
        }

        movementVector = Vector3.zero;
        
        // Get movement from WASD
        if (!doGrapple)
        {
            movementVector.x = Input.GetAxis("Horizontal");
            movementVector.z = Input.GetAxis("Vertical");
        }

        if (Input.GetMouseButton(0))
        {
            moveForwardMomentum = false;
        }
        else
        {
            moveForwardMomentum = true;
        }

        // Velocity for after un-grappling
        if (!isGrounded && moveForwardMomentum && !isAgainstWallDuringGrapple)
        {
            rb.AddForce(transform.forward * (GrappleSpeed * 0.5f), ForceMode.Impulse);
            //rb.AddForce(transform.up * (GrappleSpeed), ForceMode.Impulse);
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
            else if (stowedObject && Input.GetKey(KeyCode.F)) 
            {
                stowedObject = false;
                Ray fromCamera = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                ThrowObject(fromCamera.GetPoint(100f) - Camera.main.transform.position);
                StopMagnesis();
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
            foreach (KeyValuePair<GameObject, Material> mgo in tkObjects) { mgo.Key.GetComponent<MeshRenderer>().material = mgo.Value; }
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
        if (!Physics.Raycast (transform.position, -Vector3.up, distanceGround + 1.0f))
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

        if (heldObject != null && stowedObject)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, StowPoint.position, 0.3f);
        }
    }

    private void aim()
    {
        // This line makes it so, while aiming, the player and camera both face in the 
        // same direction and the they move together when rotating around the y axis.
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        if (!stowedObject)
        {
            // Paint moveable objects that are visible
            foreach (KeyValuePair<GameObject, Material> mgo in tkObjects)
            {
                Ray toGo = new Ray(transform.position, mgo.Key.transform.position - transform.position);
                if (!inMagnesis && Physics.Raycast(toGo, out RaycastHit hitInfo))
                {
                    if (hitInfo.collider.gameObject.tag == "Moveable" || hitInfo.collider.gameObject.tag == "Grapple")
                    {
                        mgo.Key.GetComponent<MeshRenderer>().material = HighlightedMoveable;
                    }
                    else
                    {
                        mgo.Key.GetComponent<Renderer>().material = mgo.Value;
                    }
                }
            }

            // Paint moveable object being aimed at
            if (!inMagnesis && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit aimHit, ScarfLength) && !doGrapple)
            {
                if (aimHit.collider.gameObject.tag == "Moveable" && haveLetGoOfMouse)
                {
                    inCrosshairs = aimHit.collider.gameObject;
                    aimHit.collider.GetComponent<MeshRenderer>().material = SelectedMoveable;
                    readyToGrab = true;
                }
                else if (aimHit.collider.gameObject.tag == "Grapple")
                {
                    aimHit.collider.GetComponent<MeshRenderer>().material = SelectedGrapple;
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
        else
        {
            heldObject.GetComponent<MeshRenderer>().material = SelectedMoveable;
        }
        
    }

    private void Magnesis()
    {
        // This line makes it so, while aiming, the player and camera both face in the 
        // same direction and the they move together when rotating around the y axis.
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        inMagnesis = true;
        if (heldObject == null && !stowedObject)
        {
            heldObject = inCrosshairs;
        }
        heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        heldObject.GetComponent<Rigidbody>().freezeRotation = true;
        heldObject.GetComponent<Rigidbody>().useGravity = false;
        heldObject.SendMessage("Caught");

        Ray fromCamera = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        float distanceFromCamera = Vector3.Distance(Camera.main.transform.position, Camera.main.transform.position + heldObjectOffset);
        float distanceFromPlayer = Vector3.Distance(transform.position, Camera.main.transform.position + heldObjectOffset);
        heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(distanceFromCamera), 0.3f);

        if (Input.GetKey(KeyCode.Q) && distanceFromPlayer > 5f)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, transform.position, 0.05f);
            heldObjectOffset = (transform.position - Camera.main.transform.position) + (heldObject.transform.position - transform.position);
        }
        if (Input.GetKey(KeyCode.Q) && distanceFromPlayer <= 8F)
        {
            stowedObject = true;
        }
        if (Input.GetKey(KeyCode.E) && distanceFromPlayer < ScarfLength)
        {
            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, fromCamera.GetPoint(ScarfLength + 25f), .05f);
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

        if (Input.GetKeyDown(KeyCode.F))
        {
            stowedObject = false;
            ThrowObject(fromCamera.GetPoint(100f) - Camera.main.transform.position);
        }
    }

    private void StopMagnesis()
    {
        if (!stowedObject)
        {
            if (heldObject != null)
            {
                heldObject.GetComponent<Rigidbody>().freezeRotation = false;
                heldObject.GetComponent<Rigidbody>().useGravity = true;
            }
            heldObject = null;
            inCrosshairs = null;
            haveLetGoOfMouse = false;
            readyToGrab = false;
        }
        inMagnesis = false;
    }

    private void ThrowObject(Vector3 direction)
    {
        GameObject toThrow = heldObject;
        StopMagnesis();
        toThrow.GetComponent<Rigidbody>().velocity = direction * ThrowSpeed;
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
            if (hit.collider.CompareTag("Grapple"))
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
        GrappleSpeed = 0.05f;
        //GetComponent<LineRenderer>().enabled = false;
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
        //GetComponent<LineRenderer>().enabled = true; // Show "Scarf"
        // Draw "Scarf" line
        //GetComponent<LineRenderer>().SetPosition(0, transform.position);
        //GetComponent<LineRenderer>().SetPosition(1, grappleLocation);

        rb.mass = 0.1f;
        rb.useGravity = false;
        rb.freezeRotation = true;
        haveGrappled = true;


        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = Vector3.Lerp(transform.position, grappleLocation, (GrappleSpeed += 0.05f) * Time.deltaTime);
    }

    public void AddMoveableObjectToList(GameObject toAdd)
    {
        tkObjects.Add(toAdd, toAdd.GetComponent<MeshRenderer>().material);
    }

    public void RemoveMoveableObjectFromList(GameObject toRemove)
    {
        tkObjects.Remove(toRemove);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.name == "RockForBossProjectile")
        {
            Cursor.visible = true;
            Destroy(this.gameObject);
            PlayerHealth--;
            if(PlayerHealth == 0)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void reduceHealth()
    {
        PlayerHealth = PlayerHealth - 1;

        if (PlayerHealth == 0)
        {
            GameObject boss = GameObject.Find("TestBoss");
            if (boss == null)
            {
                boss = GameObject.Find("TestBoss");
            }
            boss.SendMessage("resetGame");
            Destroy(this.gameObject);

        }

    }
}
