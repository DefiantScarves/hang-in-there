using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossInput : MonoBehaviour
{
    private GameObject Player;

    private bool canAttack; //This gets changed when the enemy can try and shoot at you (ie. within range)

    private float shootCooldown = 3.0f;

    public GameObject AttackPoint;

    public GameObject Projectile;

    public int health = 3;


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Player");
        canAttack = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canAttack)
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
    }

    private void Shoot()
    {
        Instantiate(Projectile, AttackPoint.transform.position, AttackPoint.transform.rotation);
    }

    public void reduceHealth()
    {
        health = health - 1;

        if (health == 0)
            Destroy(this.gameObject);
    }

    public void resetGame()
    {
        StartCoroutine(waiter());
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
    }

    public void Activate()
    {
        canAttack = true;
    }
}
