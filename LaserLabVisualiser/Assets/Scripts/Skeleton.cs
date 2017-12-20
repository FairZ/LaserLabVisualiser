using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Skeleton 
{

	public GameObject root;

	public Material dm_red;
	public Material dm_green;
	public Material dm_blue;

	Dictionary<int, jointData> skeleton_dict;

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
	public struct jointData
	{
		public float x, y, z;
		public int trackState;
		public int parentID;
		public jointData(float _x, float _y, float _z, int _trackState, int _parentID)
		{
			x = _x;
			y = _y;
			z = _z;
			trackState = _trackState;
			parentID = _parentID;
		}
	}
	//https://vvvv.org/sites/default/files/images/kinectskeleton-map2.png
	int getParentId(int _id)
	{
		switch(_id)
		{
			//Spine and head
			case((int)JointType.SpineBase): return -1;
			case((int)JointType.SpineMid): return (int)JointType.SpineBase;
			case((int)JointType.Neck): return (int)JointType.SpineShoulder;
			case((int)JointType.Head): return (int)JointType.Neck;
			//Left arm
			case((int)JointType.ShoulderLeft): return (int)JointType.SpineShoulder;
			case((int)JointType.ElbowLeft): return (int)JointType.ShoulderLeft;
			case((int)JointType.WristLeft): return (int)JointType.ElbowLeft;
			case((int)JointType.HandLeft): return (int)JointType.WristLeft;
			//Right arm
			case((int)JointType.ShoulderRight): return (int)JointType.SpineShoulder;
			case((int)JointType.ElbowRight): return (int)JointType.ShoulderRight;
			case((int)JointType.WristRight): return (int)JointType.ElbowRight;
			case((int)JointType.HandRight): return (int)JointType.WristRight;	
			//Left leg
			case((int)JointType.HipLeft): return (int)JointType.SpineBase;
			case((int)JointType.KneeLeft): return (int)JointType.HipLeft;
			case((int)JointType.AnkleLeft): return (int)JointType.KneeLeft;
			case((int)JointType.FootLeft): return (int)JointType.AnkleLeft;
			//Right leg
			case((int)JointType.HipRight): return (int)JointType.SpineBase;
			case((int)JointType.KneeRight): return (int)JointType.HipRight;
			case((int)JointType.AnkleRight): return (int)JointType.KneeRight;
			case((int)JointType.FootRight): return (int)JointType.AnkleRight;
			//Spine Shoulder length, I dont know why its down here either, but it's the Kinect convention
			case((int)JointType.SpineShoulder): return (int)JointType.SpineMid;
			//Left hand
			case((int)JointType.HandTipLeft): return (int)JointType.HandLeft;
			case((int)JointType.ThumbLeft): return (int)JointType.HandLeft;
			//Right hand
			case((int)JointType.HandTipRight): return (int)JointType.HandRight;
			case((int)JointType.ThumbRight): return (int)JointType.HandRight;
			default:
				return -1;
		}
	}


	public Skeleton(GameObject _root, List<Transform> joints, List<Material> _debugMats)
	{
		root = _root;
		if(_debugMats.Count > 2)
		{
			dm_red  = _debugMats[0];
			dm_green = _debugMats[1];
			dm_blue = _debugMats[2];
		}
		skeleton_dict = new Dictionary<int, jointData>();
	}

	public void updateJoints(String _data)
	{
		skeleton_dict.Clear();

		//Split the packet into the component 
		String[] jointData_str = _data.Split ('/');

		//Populate the dictionary by parsing the sent data
		foreach (string s in jointData_str) {
			if (s.Length < 2)
				continue;

			String[] coords = s.Split(';');
			int currJoint = Int32.Parse (coords [0]);


			float x = float.Parse (coords [1]);
			float y = float.Parse (coords [2]);
			float z = float.Parse (coords [3]);

			int trackState = Int32.Parse (coords [4]);

			skeleton_dict.Add (currJoint, new jointData (x, y, z, trackState, getParentId(currJoint)));
		}

		//Update the joint positions if known, and colour them accordingly
		for (int i = 0; i <= 24; i++) {
			//an empty joint data that we store our joint in if it exists
			jointData jd;
			//If there is a dictionary entry for the joint, update its position
			if (skeleton_dict.TryGetValue (i, out jd)) {
				root.transform.GetChild (i).position = new Vector3 (jd.x, jd.y, jd.z)*10;
				//If the position is known colour it blue
				if (jd.trackState == 2)
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_blue;
				//If the position is inferred colour it green
				else if(jd.trackState == 1)
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_green;
				else
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_red;
			}
			//If there is no entry colour it red
			else
				root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_red;
		}

	}
	public Dictionary<int, jointData> getSkeleDic()
	{
		Dictionary<int, jointData> ret = new Dictionary<int, jointData>(skeleton_dict);
		return ret;
	}
}
