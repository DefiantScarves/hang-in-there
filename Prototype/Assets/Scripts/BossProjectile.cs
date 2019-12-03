using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public GameObject RockExplosionPrefab;

    public float Speed = 0.1f;
    public PlayerInput pI;

    private Vector3 movementVector;
    private GameObject player;
    private GameObject boss;
    private bool caught;

    // Start is called before the first frame update
    void Start()
    {
        //movementVector = transform.forward * Speed;

        player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        boss = GameObject.Find("TestBoss");
        if (boss == null)
        {
            boss = GameObject.Find("TestBoss");
        }

        movementVector = (player.transform.position - transform.position).normalized * Speed;

        caught = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!caught)
        {
            GetComponent<Rigidbody>().MovePosition(transform.position + movementVector);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == "Player" && !caught)
        {
            collision.gameObject.SendMessage("reduceHealth");
            gameObject.tag = "Untagged";
            Instantiate(RockExplosionPrefab, transform.position, Quaternion.Euler(collision.contacts[0].normal));
            player.SendMessage("RemoveMoveableObjectFromList", gameObject);
            Destroy(this.gameObject);
        }

        else if (collision.transform.name == "TestBoss" || collision.transform.name == "Cube")
        {
            collision.gameObject.SendMessage("reduceHealth");
            gameObject.tag = "Untagged";
            Instantiate(RockExplosionPrefab, transform.position, Quaternion.Euler(collision.contacts[0].normal));
            player.SendMessage("RemoveMoveableObjectFromList", gameObject);
            Destroy(this.gameObject);
        }

        else if (collision.transform.name != "Player")
        {
            gameObject.tag = "Untagged";
            Instantiate(RockExplosionPrefab, transform.position, Quaternion.Euler(collision.contacts[0].normal));
            player.SendMessage("RemoveMoveableObjectFromList", gameObject);
            Destroy(this.gameObject);
        }
    }

    public void Caught()
    {
        caught = true;
    }
}
