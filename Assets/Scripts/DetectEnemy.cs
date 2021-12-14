using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectEnemy : MonoBehaviour
{
	Soldier soldier;
	public int updateInterval = 10;

    [Header("DEBUG: don't change")]
    public LinkedList<Collider> enemies = new LinkedList<Collider>();
    // Start is called before the first frame update
    void Start()
    {
        soldier = transform.parent.gameObject.GetComponent<Soldier>();
    }

    void OnTriggerEnter(Collider coll)
    {
        enemies.AddLast(coll);
    }

    void OnTriggerExit(Collider coll)
    {  
        enemies.Remove(coll);
    }

    // Update is called once per frame
    void Update()
    {
    	if (Time.frameCount%updateInterval != 0) return;

        if (enemies.Count > 0)
        {
            if (enemies.First.Value == null)
            {
                enemies.RemoveFirst();
                return;
            }
            if (soldier.target != enemies.First.Value.transform && !soldier.retreating) {
                soldier.target = enemies.First.Value.transform;
            } 

        } else soldier.target = null;
    }
}
