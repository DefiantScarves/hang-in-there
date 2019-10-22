using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashTrigger : MonoBehaviour
{

    public GameObject RockExplosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.name == "DemoPlayer")
        {
            gameObject.tag = "Untagged";
            Instantiate(RockExplosionPrefab, transform.position, Quaternion.Euler(collision.contacts[0].normal));
            Destroy(gameObject);
        }

    }
}
