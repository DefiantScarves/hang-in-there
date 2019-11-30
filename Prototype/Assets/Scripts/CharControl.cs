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
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        thisAnimator.SetFloat("Forwards", vertical);
        //thisAnimator.SetFloat("Backwards", -1 * vertical);
        //thisAnimator.SetFloat("Right", horizontal);
        //thisAnimator.SetFloat("Left", -1 * horizontal);
    }
}
