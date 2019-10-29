using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashTrigger : MonoBehaviour
{
    public GameObject Player;
    public float TimeBeforeReset;
    public float SpeedToFlyBackToPlayer;

    private bool weakened;
    private Color normalST;
    private Color weakenedST;

    // Start is called before the first frame update
    void Start()
    {
        weakened = false;
        normalST = new Color(255f, 0f, 0f);
        weakenedST = new Color(0f, 0f, 255f);
        TimeBeforeReset = 3f;
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
            Invoke("ResetST", TimeBeforeReset);
            collision.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();
            collision.gameObject.SendMessage("SendBackToPosition", Player.transform.position);
            weakened = true;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void ResetST()
    {
        weakened = false;
        gameObject.GetComponent<Renderer>().material.SetColor("_Color", normalST);
    }
}
