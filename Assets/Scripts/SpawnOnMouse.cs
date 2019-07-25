using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnOnMouse : MonoBehaviour
{

	public GameObject squad;

    public Checkpoint[] checkpoints;

    public LayerMask rayMask;

    public int agents = 0;

    Vector2 orgBoxPos = Vector2.zero;
    Vector2 endBoxPos = Vector2.zero;

    public int turboSpawnRate = 4;

    public Texture selectionTexture;

    Camera cam;

    [HideInInspector]
    public bool soldiersSelected = false;

    public GameObject selectionEffect;

    public int maxSquads = 35;
    int spawnedSquads = 0;

    float timeHeld;

    void Start()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            soldiersSelected = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<SquadAI>().DeselectSoldiers();
            }
        }

        if (soldiersSelected && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, rayMask))
            {
                Instantiate(selectionEffect, hit.point, Quaternion.identity);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetComponent<SquadAI>().MoveSelectedSoldiers(hit.point);
                } 
            }
        }


        if (Input.GetMouseButton(1) && spawnedSquads < maxSquads)
        {
            timeHeld += Time.deltaTime;
            if (timeHeld >= 1f/turboSpawnRate)
            {
                timeHeld = 0f;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, rayMask))
                {
                    spawnedSquads++;
                    GameObject newSquad = PhotonNetwork.Instantiate("Squad", transform.position, Quaternion.identity);
                    newSquad.transform.parent = transform;
                    newSquad.GetComponent<SquadAI>().checkpoints = checkpoints;
                    newSquad.GetComponent<SquadAI>().SetDestination(hit.point);
                    newSquad.GetComponent<SquadAI>().spawner = this;
                    newSquad.GetComponent<SquadAI>().orderOfArrival = 5*agents;
                    agents += newSquad.GetComponent<SquadAI>().size;
                }
            }    
        } else {
            timeHeld = 1f/turboSpawnRate;
        }

        if (soldiersSelected)
        {
            orgBoxPos = Vector2.zero;
            endBoxPos = Vector2.zero;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            orgBoxPos = Input.mousePosition;
        } else if (Input.GetMouseButton(0))
        {
            endBoxPos = Input.mousePosition;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<SquadAI>().HighlightSoldiers(GetViewportBounds(), cam);
            }
        } else if (Input.GetMouseButtonUp(0)) {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<SquadAI>().SelectSoldiers();
            }
        } else {
            orgBoxPos = Vector2.zero;
            endBoxPos = Vector2.zero;
        }

    }

    void OnGUI()
    {
        if (orgBoxPos != Vector2.zero && endBoxPos != Vector2.zero) {
            GUI.DrawTexture(new Rect(orgBoxPos.x, Screen.height - orgBoxPos.y, endBoxPos.x - orgBoxPos.x, -1 * ((Screen.height - orgBoxPos.y) - (Screen.height - endBoxPos.y))), selectionTexture);
        }
    }

    public Bounds GetViewportBounds()
    {
        var v1 = cam.ScreenToViewportPoint( orgBoxPos );
        var v2 = cam.ScreenToViewportPoint( Input.mousePosition );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = cam.nearClipPlane;
        max.z = cam.farClipPlane;
 
        var bounds = new Bounds();
        bounds.SetMinMax( min, max );
        return bounds;
    }


    [PunRPC]
    public void EnemyGunshot(Vector3 position)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SquadAI>().Gunshot(position);
        }
    }

    public void Gunshot(Vector3 shooter)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SquadAI>().Gunshot(shooter);
        }
    }
}
