using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnMouse : MonoBehaviour
{

	public GameObject squad;

    public Checkpoint[] checkpoints;

    public LayerMask rayMask;

    public int agents = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
        	var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        	RaycastHit hit;

        	if (Physics.Raycast(ray, out hit, Mathf.Infinity, rayMask))
        	{
        		GameObject newSquad = Instantiate(squad, transform);
                newSquad.GetComponent<SquadAI>().checkpoints = checkpoints;
        		newSquad.GetComponent<SquadAI>().SetDestination(hit.point);
                newSquad.GetComponent<SquadAI>().spawner = this;
                newSquad.GetComponent<SquadAI>().orderOfArrival = 5*agents;
                agents += newSquad.GetComponent<SquadAI>().size;
            }
        }
    }
}
