using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashTrigger : MonoBehaviour
{
    public GameObject Player;
    public GameObject Rune;
    public GameObject RockExplosionPrefab;
    public Material StandardColor;
    public Material WeakenedColor;
    public float TimeBeforeReset;

    private bool weakened;
    private GameObject cage;

    // Start is called before the first frame update
    void Start()
    {
        weakened = false;
        TimeBeforeReset = 3f;
        cage = GameObject.Find("Cage");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!weakened)
        {
            Rune.GetComponent<MeshRenderer>().material = WeakenedColor;
            Invoke("ResetST", TimeBeforeReset);
            collision.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();
            collision.gameObject.SendMessage("SendBackToPosition", Player.transform.position, SendMessageOptions.DontRequireReceiver);
            weakened = true;
        }
        else
        {
            Instantiate(RockExplosionPrefab, transform.position, Quaternion.Euler(collision.contacts[0].normal));
            Destroy(this.gameObject);
        }
    }

    private void ResetST()
    {
        weakened = false;
        Rune.GetComponent<MeshRenderer>().material = StandardColor;
    }

    private void OnDestroy()
    {
        cage.SendMessage("BreakLock");
    }
}
