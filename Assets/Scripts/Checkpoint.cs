using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Checkpoint : MonoBehaviourPun
{

	public bool canCapture = true;
	public Color friendlyCapturingColor;
	public Color friendlyCapturedColor;
	public Color enemyCapturingColor;
	public Color enemyCapturedColor;
	public Color defaultColor;
	public float captureTime;
	public int frameInterval = 2;
	PhotonView photonView;

	public int control; //-1 = enemy, 0 = neutral, 1 = friendly

    [Header("DEBUG: don't change")]
	public float radius;
	int soldiersCapturing = 0;
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
    	coll.GetComponent<Soldier>().squad.EnterCheckpoint(coll.GetComponent<Soldier>(), this);
    	if (soldiersCapturing == 0) photonView.RPC("CaptureStatus", RpcTarget.All, false);
    	soldiersCapturing++;
    }

    void OnTriggerExit(Collider coll)
    {
    	coll.GetComponent<Soldier>().squad.ExitCheckpoint(coll.GetComponent<Soldier>());
    	soldiersCapturing--;
    	if (soldiersCapturing == 0) photonView.RPC("CaptureStatus", RpcTarget.All, true);
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

    [PunRPC]
    public void CaptureStatus(bool status)
    {
    	canCapture = status;
    }

    void Update()
    {
    	if (Time.frameCount%frameInterval != 0) return;
    	if (canCapture && soldiersCapturing == 0)
    	{
    		firstEnemyCycle = true;
    		firstFriendlyCycle = true;

    		enemyCapturingTime = 0f;
    		capturingTime = 0f;

    	} else if (!canCapture && soldiersCapturing == 0 && render.material.color != enemyCapturedColor) {
    		enemyCapturingTime += Time.deltaTime;

    		if (firstEnemyCycle) ResetEnemy();

    		if (enemyCapturingTime >= captureTime) {
    			render.material.color = enemyCapturedColor;
    			control = -1;
    		} else {
    			render.material.color = Color.Lerp(originalColor, enemyCapturingColor, enemyCapturingTime/captureTime);
    		}
    	} else if (canCapture && soldiersCapturing != 0 && render.material.color != friendlyCapturedColor) {
    		capturingTime += Time.deltaTime;

    		if (firstFriendlyCycle) ResetFriendly();

    		if (capturingTime >= captureTime) {
    			render.material.color = friendlyCapturedColor;
    			control = 1;
       		} else {
    			render.material.color = Color.Lerp(originalColor, friendlyCapturingColor, capturingTime/captureTime);
       		}
    	} else if (!canCapture && soldiersCapturing != 0) {
    		firstEnemyCycle = true;
    		firstFriendlyCycle = true;
    		if (control == 1) {
    			render.material.color = friendlyCapturedColor;
    		} else if (control == 0) {
    			render.material.color = defaultColor;
    		} else {
    			render.material.color = enemyCapturedColor;
    		}
    	}
    }

}
