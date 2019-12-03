using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControl : MonoBehaviour
{
    private Animator thisAnimator;

    // Start is called before the first frame update
    void Start()
    {
        thisAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Mathf.Abs( Input.GetAxis("Vertical")) * 0.5f;

        if (Input.GetKey(KeyCode.LeftShift)) vertical *= 2f;


        thisAnimator.SetFloat("Forwards", vertical);
        //thisAnimator.SetFloat("Backwards", -1 * vertical);
        //thisAnimator.SetFloat("Right", horizontal);
        //thisAnimator.SetFloat("Left", -1 * horizontal);
    }
}
