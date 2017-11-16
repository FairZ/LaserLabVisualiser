using UnityEngine;
using System.Collections;

public class LookingScript : MonoBehaviour {

	RaycastHit m_hit;

	void Update () 
	{
		if (Physics.Raycast (transform.position, transform.forward, out m_hit)) 
		{
			m_hit.collider.gameObject.GetComponentInChildren<HUDLerp>().LookedAt();
		}

	}

}
