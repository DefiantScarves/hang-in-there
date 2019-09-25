using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 10f;

    private Rigidbody rb;

    public LayerMask groundLayers;

    public float jumpForce = 7;

    public CapsuleCollider col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementVector = Vector3.zero;
        movementVector.x = Input.GetAxis("Horizontal");
        movementVector.z = Input.GetAxis("Vertical");

        // Make movement happen in direction the camera is facing
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Speed = Speed * 2;
        }


        movementVector = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * movementVector;
        GetComponent<Rigidbody>().AddForce(movementVector * Speed);

        if (movementVector != Vector3.zero)
        {
            transform.forward = movementVector;
        }

        rb.AddForce(movementVector * Speed);

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
}
