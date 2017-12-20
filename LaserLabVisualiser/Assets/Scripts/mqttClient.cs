using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mqttClient : MonoBehaviour {

	public mqttBroker m_broker;

	public string m_topic;

	protected string m_data = "";

	public bool offline = false;

	public string filePath = "";

	protected void Start()
	{
		//m_broker = GameObject.FindGameObjectWithTag("Broker").GetComponent<mqttBroker>();
		if(!offline)
			m_broker.SubscribeToTopic(m_topic, this);
	}

	public void TransferPayload(string _topic, string _payload)
	{
		m_data = _payload;
	}
}