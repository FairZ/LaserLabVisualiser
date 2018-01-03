﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// Kalman filter code taken from
/// https://github.com/asus4/UnityIMU
/// </summary>
using Kalman;

public class Skeleton 
{

	public GameObject root;

	public Material dm_red;
	public Material dm_green;
	public Material dm_blue;

	List<IKalmanWrapper> kalman;

	Dictionary<int, JointData> prev_skeleton_dict;
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

	int getChildId(int _id)
	{
		switch(_id)
		{
		//Spine and head
		case((int)JointType.SpineBase): return -1;
		case((int)JointType.SpineMid): return -1;
		case((int)JointType.Neck): return (int)JointType.Head;
		case((int)JointType.Head): return -1;
			//Left arm
		case((int)JointType.ShoulderLeft): return (int)JointType.ElbowLeft;
		case((int)JointType.ElbowLeft): return (int)JointType.WristLeft;
		case((int)JointType.WristLeft): return (int)JointType.HandLeft;
		case((int)JointType.HandLeft): return (int)JointType.HandTipLeft;
			//Right arm
		case((int)JointType.ShoulderRight): return (int)JointType.ElbowRight;
		case((int)JointType.ElbowRight): return (int)JointType.WristRight;
		case((int)JointType.WristRight): return (int)JointType.HandRight;
		case((int)JointType.HandRight): return (int)JointType.HandTipRight;	
			//Left leg
		case((int)JointType.HipLeft): return (int)JointType.KneeLeft;
		case((int)JointType.KneeLeft): return (int)JointType.AnkleLeft;
		case((int)JointType.AnkleLeft): return (int)JointType.FootLeft;
		case((int)JointType.FootLeft): return -1;
			//Right leg
		case((int)JointType.HipRight): return (int)JointType.KneeRight;
		case((int)JointType.KneeRight): return (int)JointType.AnkleRight;
		case((int)JointType.AnkleRight): return (int)JointType.FootRight;
		case((int)JointType.FootRight): return -1;
			//Spine Shoulder length, I dont know why its down here either, but it's the Kinect convention
		case((int)JointType.SpineShoulder): return -1;
			//Left hand
		case((int)JointType.HandTipLeft): return -1;
		case((int)JointType.ThumbLeft): return -1;
			//Right hand
		case((int)JointType.HandTipRight): return -1;
		case((int)JointType.ThumbRight): return -1;
		default:
			return -1;
		}
	}

	public Skeleton(GameObject _root, List<Material> _debugMats)
	{
		root = _root;
		if(_debugMats.Count > 2)
		{
			dm_red  = _debugMats[0];
			dm_green = _debugMats[1];
			dm_blue = _debugMats[2];
		}
		curr_skeleton_dict = new Dictionary<int, JointData>();
		prev_skeleton_dict = new Dictionary<int, JointData>();


		kalman = new List<IKalmanWrapper>();
		for(int i = 0; i <= 24; i++)
		{
			kalman.Add(new MatrixKalmanWrapper());
		}
	}

	Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
	{
		Vector3 P = x * Vector3.Normalize(B - A) + A;
		return P;
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
			if (curr_skeleton_dict.TryGetValue (currJoint, out jd)) {
				curr_skeleton_dict.Remove (currJoint);
				jd.pos = new Vector3 (x, y, z);
				jd.trackState = trackState;
				curr_skeleton_dict.Add (currJoint, jd);
			}
			else
				curr_skeleton_dict.Add (currJoint, new JointData (x, y, z, trackState, getParentId(currJoint)));
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
				//Vector3 prev_pos = new Vector3(jd_prev.x, jd_prev.y, jd_prev.z);


				//float dist_moved = Vector3.Distance(tracked_pos, prev_pos);

				//If the position is known colour it blue
				if(jd.trackState == 2)
				{
					Vector3 tracked_pos = jd.pos;

					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_blue;

					//Update our average measurements with newly tracked data
					JointData parentJoint;
					if (curr_skeleton_dict.TryGetValue (jd.parentID, out parentJoint)) 
					{
						jd.updateDistToParent (parentJoint);
					}
					//Update direction based on last child direction if possible
					JointData childJoint;
					if (curr_skeleton_dict.TryGetValue (getChildId (i), out childJoint) && curr_skeleton_dict.TryGetValue (jd.parentID, out parentJoint)) 
					{
						jd.updateDirChild (childJoint);

						Ray r = new Ray();
						r.origin = parentJoint.pos;
						r.direction = jd.dir;
						final = r.GetPoint(jd.distToParent);
					} 
					//Update based on parents, less preferable
					else if(curr_skeleton_dict.TryGetValue (jd.parentID, out parentJoint)) 
					{
						jd.updateDirParent (parentJoint);

						if (Vector3.Distance (tracked_pos, parentJoint.pos) > jd.distToParent * 1.1f) 
						{
							Ray r = new Ray();
							r.origin = parentJoint.pos;
							r.direction = jd.dir;
							final = r.GetPoint(jd.distToParent);
						}
					}



					curr_skeleton_dict.Remove (i);
					curr_skeleton_dict.Add (i, jd);

					final = kalman[i].Update(tracked_pos);
				}

				//If the position is inferred colour it green
				else if(jd.trackState == 1)
				{
					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_green;

					JointData parentJoint, parentOfParentJoint;
					if(curr_skeleton_dict.TryGetValue(jd.parentID, out parentJoint))
					{
						if(curr_skeleton_dict.TryGetValue(parentJoint.parentID, out parentOfParentJoint))
						{
							Ray r = new Ray();
							r.origin = parentJoint.pos;
							r.direction = jd.dir;
							final = r.GetPoint(jd.distToParent);
						}
					}
				}
				else
					root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_red;



				root.transform.GetChild (i).position = final*10;//Scaled up to see clearer
			}
			//If there is no entry colour it red
			else
				root.transform.GetChild (i).gameObject.GetComponent<MeshRenderer> ().material = dm_red;
		}
			

		prev_skeleton_dict = new Dictionary<int, JointData>(curr_skeleton_dict);
	}

	public Dictionary<int, JointData> getSkeleDic()
	{
		Dictionary<int, JointData> ret = new Dictionary<int, JointData>(curr_skeleton_dict);
		return ret;
	}
}
