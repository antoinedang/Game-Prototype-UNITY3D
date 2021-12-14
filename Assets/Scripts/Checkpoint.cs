using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Checkpoint : MonoBehaviourPun
{

	public bool canCapture = true;
	public float captureTime;
	public int frameInterval = 2;
	public Color friendlyCapturingColor;
	public Color friendlyCapturedColor;
	public Color enemyCapturingColor;
	public Color enemyCapturedColor;
	public Color defaultColor;
	PhotonView photonView;

	public int control; //-1 = enemy, 0 = neutral, 1 = friendly

    [Header("DEBUG: don't change")]
	public float radius;
	int enemies = 0;
	int soldiers = 0;
	Renderer render;
	float capturingTime;
	float enemyCapturingTime;
	Color originalColor;
	bool firstFriendlyCycle = false;
	bool firstEnemyCycle = false;

    //use the local enemies + players to determine what is happening
    //communicate to the server to determine what is happening (tell them if a soldier is on the checkpoint, have a boolean that shows if they can charge or not)

	void Start()
	{
		photonView = GetComponent<PhotonView>();
		render = GetComponent<Renderer>();
		originalColor = defaultColor;
		radius = GetComponent<SphereCollider>().radius*transform.localScale.x;
	}

    void OnTriggerEnter(Collider coll)
    {
		print(coll.gameObject.name);
		if (coll.gameObject.layer == 10) enemies++;
		else soldiers++;
    }

    void OnTriggerExit(Collider coll)
    {
		print(coll.gameObject.name);
		if (coll.gameObject.layer == 10) enemies--;
		else soldiers--;
    }

    void ResetFriendly()
    {
    	originalColor = render.material.color;
    	capturingTime = 0f;
    	enemyCapturingTime = 0f;
    	firstFriendlyCycle = false;
    	firstEnemyCycle = true;
    }

    void ResetEnemy()
    {
    	originalColor = render.material.color;
    	capturingTime = 0f;
    	enemyCapturingTime = 0f;
    	firstFriendlyCycle = true;
    	firstEnemyCycle = false;
    }

    void Update()
    {
		if (!canCapture) return;
    	if (Time.frameCount%frameInterval != 0) return;
		if (soldiers + enemies == 0 && control != 0) {
			control = 0;
    		enemyCapturingTime = 0f;
    		capturingTime = 0f;
    		firstEnemyCycle = false;
    		firstFriendlyCycle = false;
    		render.material.color = defaultColor;
			originalColor = defaultColor;
    	} else if (soldiers < enemies && control != -1) {
    		if (capturingTime != 0) ResetEnemy();
    		capturingTime = 0f;
    		enemyCapturingTime += Time.deltaTime;


    		if (enemyCapturingTime >= captureTime) {
    			render.material.color = enemyCapturedColor;
    			control = -1;
    		} else {
    			render.material.color = Color.Lerp(originalColor, enemyCapturingColor, enemyCapturingTime/captureTime);
    		}
    	} else if (soldiers > enemies && control != 1) {
			
    		if (enemyCapturingTime != 0) ResetFriendly();
    		enemyCapturingTime = 0f;
    		capturingTime += Time.deltaTime;

    		if (capturingTime >= captureTime) {
    			render.material.color = friendlyCapturedColor;
    			control = 1;
       		} else {
    			render.material.color = Color.Lerp(originalColor, friendlyCapturingColor, capturingTime/captureTime);
       		}
    	}
    }

}
