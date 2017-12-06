using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class mqttSkeleton : mqttClient 
{

	public Material red;
	public Material green;
	public Material blue;

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
	struct jointData
	{
		public float x, y, z;
		public int trackState;

		public jointData(float _x, float _y, float _z, int _trackState)
		{
			x = _x;
			y = _y;
			z = _z;
			trackState = _trackState;
		}
	}

	private GameObject activeSkeleton;


	void createSkeleton()
	{
		//Create a sphere to use as a template
		GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		sphere.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
		//Create a gameobject to attach the joints to
		GameObject parent = new GameObject("Skeleton");
		//24 is length of joint types
		//Each iteration make a copy of the template sphere and attach it to the parent and name it
		for (uint i = 0; i < 24; i++) {
			GameObject tmpGO = Instantiate (sphere);
			//Cast to our enum to determine name
			JointType jt = (JointType)i;
			tmpGO.name = jt.ToString ();
			tmpGO.transform.SetParent (parent.transform);
		}
		//Finally set the template to be the final joint, more efficient
		JointType _jt = (JointType)24;
		sphere.name = _jt.ToString ();
		sphere.transform.SetParent (parent.transform);
		//Set the active skeleton to be our parent
		activeSkeleton = parent;
	}


	void updateJoints()
	{
		//Create Dictionary to assign the Joint data to the joint (cast key to JointType for nicer formatting)
		Dictionary<int, jointData> skeleton_dict = new Dictionary<int, jointData>();
		//Split the packet into the component 
		String[] jointData_str = m_data.Split ('/');

		//Populate the dictionary by parsing the sent data
		foreach (string s in jointData_str) {
			if (s.Length == 0)
				continue;
			
			String[] coords = s.Split(';');
			int currJoint = Int32.Parse (coords [0]);


			float x = float.Parse (coords [1]);
			float y = float.Parse (coords [2]);
			float z = float.Parse (coords [3]);

			int trackState = Int32.Parse (coords [4]);

			Debug.Log (trackState);

			skeleton_dict.Add (currJoint, new jointData (x, y, z, trackState));
		}
			
		//Update the joint positions if known, and colour them accordingly
		for (int i = 0; i <= 24; i++) {

			jointData jd;
			//If there is a dictionary entry for the joint, update its position
			if (skeleton_dict.TryGetValue (i, out jd)) {
				activeSkeleton.transform.GetChild (i).position = new Vector3 (jd.x, jd.y, jd.z)*10;
			//If the position is known colour it blue
			if (jd.trackState == 2)
				activeSkeleton.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = blue;
			//If the position is inferred colour it green
			else if(jd.trackState == 1)
				activeSkeleton.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = green;
			else
				activeSkeleton.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = red;
			}
			//If there is no entry colour it red
			else
				activeSkeleton.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = red;
		}

	}


	void Awake()
	{
		createSkeleton ();
	}


	// Update is called once per frame
	void Update () 
	{
		if (m_data != "") 
		{
			
			updateJoints();
		}
	}
}
