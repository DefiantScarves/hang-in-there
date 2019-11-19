using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    public float SpeedToFlyBackToPlayer;

    private bool SentFromSmashTrigger;
    private Vector3 MoveToHere;

    // Start is called before the first frame update
    void Start()
    {
        SentFromSmashTrigger = false;
        SpeedToFlyBackToPlayer = 0.025f;
    }

    // Update is called once per frame
    void Update()
    {
        if (SentFromSmashTrigger)
        {
            transform.position = Vector3.Slerp(transform.position, MoveToHere, SpeedToFlyBackToPlayer);
        }
        if (Vector3.Distance(transform.position, MoveToHere) < 1f)
        {
            SentFromSmashTrigger = false;
        }
        
    }

    public void SendBackToPosition(Vector3 position)
    {
        MoveToHere = position;
        SentFromSmashTrigger = true;
    }

    public void Caught()
    {
        SentFromSmashTrigger = false;
    }
}
