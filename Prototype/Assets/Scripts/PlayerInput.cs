using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Speed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementVector = Vector3.zero;
        movementVector.x = Input.GetAxis("Horizontal");
        movementVector.z = Input.GetAxis("Vertical");

        // Make movement happen in direction the camera is facing
        movementVector = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * movementVector;
        GetComponent<Rigidbody>().AddForce(movementVector * Speed);

        if (movementVector != Vector3.zero)
        {
            transform.forward = movementVector;
        }
    }
}
