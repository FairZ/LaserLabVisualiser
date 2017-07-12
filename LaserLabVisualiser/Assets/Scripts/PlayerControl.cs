using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public float m_rotateSpeed = 0.7f;
	Vector2 m_prevPosition;
	bool m_collided;

	void Update()
	{
		transform.Rotate (0, Input.GetAxis("RightStickX") * m_rotateSpeed, 0);

		transform.Translate (Input.GetAxis ("LeftStickX")*0.01f, 0, Input.GetAxis ("LeftStickY")*-0.01f);

		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButton("Start Button")) {
			Application.Quit();
		}
	}

}
