using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class mqttSkeleton : mqttClient 
{
	///The types of joints of a Body.
	public enum JointType
	{
		SpineBase     = 0,
		SpineMid      = 1,
		Neck          = 2,
		Head          = 3,
		ShoulderLeft  = 4,
		ElbowLeft     = 5,
		WristLeft     = 6,
		HandLeft      = 7,
		ShoulderRight = 8,
		ElbowRight    = 9,
		WristRight    = 10,
		HandRight     = 11,
		HipLeft       = 12,
		KneeLeft      = 13,
		AnkleLeft     = 14,
		FootLeft      = 15,
		HipRight      = 16,
		KneeRight     = 17,
		AnkleRight    = 18,
		FootRight     = 19,
		SpineShoulder = 20,
		HandTipLeft   = 21,
		ThumbLeft     = 22,
		HandTipRight  = 23,
		ThumbRight    = 24
	}	 
	public Dictionary<JointType, Vector3> jointPositions;

	void positionJoint()
	{
		String[] joints = m_data.Split ('/');
		foreach (string s in joints) {
			String[] coords = s.Split(';');
			int currJoint = Int32.Parse (coords [0]);
			float x = float.Parse (coords [1]);
			float y = float.Parse (coords [2]);
			float z = float.Parse (coords [3]);
			Debug.Log ((JointType)Int32.Parse (coords [0]));

			transform.GetChild(currJoint).position = new Vector3(x,y,z)*10;
		}

	}

	// Update is called once per frame
	void Update () 
	{
		if (m_data != "") 
		{
			
			positionJoint();
		}
	}
}
