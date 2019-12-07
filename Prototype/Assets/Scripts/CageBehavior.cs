using System.Collections.Generic;
using UnityEngine;

public class CageBehavior : MonoBehaviour
{
    public GameObject Lock1;
    public GameObject Lock2;
    public GameObject Lock3;
    public GameObject Lock4;
    public GameObject RockExplosionPrefab;
    public float CageFlySpeed;

    private Stack<GameObject> locks;
    private int numLocks;
    private bool unlocked;
    private GameObject boss;
    
    // Start is called before the first frame update
    void Start()
    {
        locks = new Stack<GameObject>();
        locks.Push(Lock1);
        locks.Push(Lock2);
        locks.Push(Lock3);
        locks.Push(Lock4);
        numLocks = locks.Count;
        unlocked = false;
        CageFlySpeed = 20f;
        boss = GameObject.Find("TestBoss");
    }

    // Update is called once per frame
    void Update()
    {
        if (unlocked)
        {
            transform.position = transform.position + new Vector3(0f, CageFlySpeed, 0f);
        }
    }

    public void BreakLock()
    {
        GameObject toBreak = locks.Pop();
        Transform position = toBreak.transform;
        numLocks--;
        Destroy(toBreak);
        Instantiate(RockExplosionPrefab, position);
        if (numLocks == 0)
        {
            removeCage();
        }
    }

    private void removeCage()
    {
        unlocked = true;
        Invoke("DestroyCage", 5);
    }

    private void DestroyCage()
    {
        boss.SendMessage("Activate");
        Destroy(this.gameObject);
    }
}
