using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInput : MonoBehaviour
{
    private GameObject Player;

    private bool canAttack; //This gets changed when the enemy can try and shoot at you (ie. within range)

    private float shootCooldown = 3.0f;

    public GameObject AttackPoint;

    public GameObject Projectile;



    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("DemoPlayer");
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Player.transform);

        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }

        else
        {
            Shoot();
            shootCooldown = 3.0f;
        }

        
    }

    private void Shoot()
    {
        Instantiate(Projectile, AttackPoint.transform.position, AttackPoint.transform.rotation);
    }
}
