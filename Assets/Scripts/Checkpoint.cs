using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

	public bool canCapture = true;
	int soldiersCapturing = 0;
	public int control; //-1 = enemy, 0 = neutral, 1 = friendly

	[HideInInspector]
	public float radius;

    //use the local enemies + players to determine what is happening
    //communicate to the server to determine what is happening (tell them if a soldier is on the checkpoint, have a boolean that shows if they can charge or not)

	void Start()
	{
		radius = GetComponent<SphereCollider>().radius*transform.localScale.x;
	}

    void OnTriggerEnter(Collider coll)
    {
    	coll.GetComponent<Soldier>().squad.EnterCheckpoint(coll.GetComponent<Soldier>(), this);
    	soldiersCapturing++;
    }

    void OnTriggerExit(Collider coll)
    {
    	coll.GetComponent<Soldier>().squad.ExitCheckpoint(coll.GetComponent<Soldier>());
    	soldiersCapturing--;
    }

}
