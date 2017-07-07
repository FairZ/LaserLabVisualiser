using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MqttLib;


//class acting as a proxy for the mqtt Broker within unity
public class mqttBroker : MonoBehaviour {

	//IMqtt class to interact with mqtt broker
	private IMqtt m_client;
	//dictionary holding references to all subscribed gameobjects according to which topics they subscribe to
	private Dictionary<string,List<mqttClient>> m_subscribers = new Dictionary<string, List<mqttClient>>();

	//Awake function connects to the mqtt broker and sets a function to be called whenever a publish arrives
	//NOTE: a factory class may be used to allow connections to any mqtt broker not just a hard coded one
	void Awake () {
		m_client = MqttClientFactory.CreateClient ("tcp://127.0.0.1:1883", "UnityProxy");
		m_client.Connect(true);
		m_client.PublishArrived += new PublishArrivedDelegate(MessageRecieved);
	}

	//Message Recieved function is called whenever a publish is recieved, it sends the payload to any and all gameobjects subscribed to the published topic
	bool MessageRecieved(object sender, PublishArrivedArgs e)
	{
		if (m_subscribers.ContainsKey(e.Topic)) 
		{
			foreach (mqttClient g in m_subscribers[e.Topic]) 
			{
				g.TransferPayload (e.Topic, e.Payload);
			}
		}
		return true;
	}

	//subscribe to topic function is called by mqtt client classes on other gameobjects
	//it subscribes the proxy to the given topic and creates an entry for the subscribing gameobject in the subscribers dictionary
	public void SubscribeToTopic(string _topic , mqttClient _client)
	{
		if(!m_subscribers.ContainsKey(_topic)) 
		{
			m_client.Subscribe(_topic, QoS.BestEfforts);
			m_subscribers.Add(_topic, new List<mqttClient> ());
		}

		m_subscribers[_topic].Add(_client);
	}
}
