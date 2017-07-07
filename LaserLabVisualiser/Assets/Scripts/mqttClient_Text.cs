using UnityEngine;
using System.Collections;

public class mqttClient_Text : mqttClient {

	TextMesh m_textMesh;

	new void Start(){
		base.Start ();
		Initialise ();
	}

	void Update()
	{
		m_textMesh.text = m_data;
	}

	void Initialise(){
		m_textMesh = transform.GetComponent<TextMesh>();
	}
}
