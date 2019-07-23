using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectEnemy : MonoBehaviour
{
	Soldier soldier;
	public int updateInterval = 10;
    LinkedList<Collider> enemies = new LinkedList<Collider>();

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
        if (soldier.retreat) soldier.StopAttacking();

        if (enemies.Count > 0)
        {
            if (enemies.First.Value == null)
            {
                enemies.RemoveFirst();

                if (enemies.Count == 0 && soldier.attacking) soldier.StopAttacking();
                return;
            }
            if (soldier.target != enemies.First.Value.transform && !soldier.retreat) soldier.Attack(enemies.First.Value.transform);

        } else if (enemies.Count == 0 && soldier.attacking) soldier.StopAttacking();
    }
}
