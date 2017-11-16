using UnityEngine;
using System.Collections;

public class LookAtPlayer : MonoBehaviour {

	Vector3 m_directionToPlayer;
	Quaternion m_rotation;
	public GameObject m_player;


	//This should be a subroutine
	void Update () 
	{
		if(m_player != null)
		{
			transform.LookAt (m_player.transform);
			transform.Rotate (new Vector3(1.0f,0,0), 90);
		}
		else
		{
			Debug.LogWarning("m_player is not assigned");
		}
	}
}
