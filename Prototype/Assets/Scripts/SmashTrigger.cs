using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashTrigger : MonoBehaviour
{
    public GameObject Player;

    private bool weakened;
    private Color normalST;
    private Color weakenedST;

    // Start is called before the first frame update
    void Start()
    {
        weakened = false;
        normalST = new Color(255f, 0f, 0f);
        weakenedST = new Color(0f, 0f, 255f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!weakened)
        {
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", weakenedST);
            Invoke("ResetST", 3f);
            collision.gameObject.SendMessage("SendBackToPosition", Player.transform.position);
        }
        else
        {
        }
    }

    private void ResetST()
    {
        weakened = false;
        gameObject.GetComponent<Renderer>().material.SetColor("_Color", normalST);
    }
}
