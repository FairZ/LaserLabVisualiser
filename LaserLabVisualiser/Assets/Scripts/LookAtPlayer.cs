using UnityEngine;
using System.Collections;

public class LookAtPlayer : MonoBehaviour {

	Vector3 m_directionToPlayer;
	Quaternion m_rotation;
	public GameObject m_player;

	void Update () {
		transform.LookAt (m_player.transform);
		transform.Rotate (new Vector3(1.0f,0,0), 90);
	}
}
