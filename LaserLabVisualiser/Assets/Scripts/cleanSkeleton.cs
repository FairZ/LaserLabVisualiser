using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class cleanSkeleton 
{

	public GameObject root;

	public Material dm_white;
	public Material dm_black;

	Dictionary<int, JointData> curr_skeleton_dict;

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

	public cleanSkeleton(GameObject _root, List<Material> _debugMats)
	{
		root = _root;
		if(_debugMats.Count > 3)
		{
			dm_white  = _debugMats[4];
			dm_black = _debugMats [3];
		}
		curr_skeleton_dict = new Dictionary<int, JointData>();
	}

	public void updateJoints(String _data)
	{
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

			JointData jd;
			//If there is a dictionary entry for the joint, update its position
			if (curr_skeleton_dict.TryGetValue (currJoint, out jd)) 
			{
				curr_skeleton_dict.Remove (currJoint);
				jd.pos = new Vector3 (x, y, z);
				jd.trackState = trackState;
				curr_skeleton_dict.Add (currJoint, jd);
			}
			else
				curr_skeleton_dict.Add (currJoint, new JointData (x, y, z, trackState, currJoint));
		}

		//Update the joint positions if known, and colour them accordingly
		for (int i = 0; i <= 24; i++) 
		{
			//an empty joint data that we store our joint in if it exists
			JointData jd;

			//If there is a dictionary entry for the joint, update its position
			if (curr_skeleton_dict.TryGetValue (i, out jd)) 
			{
				Vector3 final = jd.pos;
				if (jd.trackState == 2) {
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_white;
					root.transform.GetChild (i).position = final*10  + new Vector3(-10,0,0);//Scaled up to see clearer
				}
				else
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_black;
			}
		}
	}

	public Dictionary<int, JointData> getSkeleDic()
	{
		Dictionary<int, JointData> ret = new Dictionary<int, JointData>(curr_skeleton_dict);
		return ret;
	}
}
