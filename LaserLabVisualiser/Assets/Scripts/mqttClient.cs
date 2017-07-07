using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mqttClient : MonoBehaviour {

	public mqttBroker m_broker;

	public string m_topic;

	protected string m_data = "No Published Data";

	protected void Start()
	{
		m_broker = GameObject.FindGameObjectWithTag("Broker").GetComponent<mqttBroker>();
		m_broker.SubscribeToTopic(m_topic, this);
	}

	public void TransferPayload(string _topic, string _payload)
	{
		m_data = _payload;
	}
}