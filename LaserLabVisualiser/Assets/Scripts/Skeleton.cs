using UnityEngine;
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

	uint avPrecision = 5;

	Dictionary<int, jointData> prev_skeleton_dict;
	Dictionary<int, jointData> curr_skeleton_dict;

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
		public float distToParent;
		public List<float> distToParentMeasurements;
		public int trackState;
		public int parentID;
		public jointData(float _x, float _y, float _z, int _trackState, int _parentID)
		{
			x = _x;
			y = _y;
			z = _z;
			distToParent = 0.0f;

			distToParentMeasurements = new List<float>();

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


	public Skeleton(GameObject _root, List<Material> _debugMats)
	{
		root = _root;
		if(_debugMats.Count > 2)
		{
			dm_red  = _debugMats[0];
			dm_green = _debugMats[1];
			dm_blue = _debugMats[2];
		}
		curr_skeleton_dict = new Dictionary<int, jointData>();
		prev_skeleton_dict = new Dictionary<int, jointData>();


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

	float replaceInAverage(List<float> _averages, float _currAverage, float _newValue)
	{
		float ret = _newValue;
		if(_averages.Count > 0)
			ret = (_averages.Count * _currAverage - _averages[0] + _newValue) / _averages.Count;
		
		_averages.Add(_newValue);

		if(_averages.Count > avPrecision)
			_averages.RemoveAt(0);
		return ret;
	}

	void updateDistToParent(ref jointData _joint, jointData _parent)
	{
		Vector3 jointPos = new Vector3(_joint.x, _joint.y, _joint.z);
		Vector3 parentPos = new Vector3(_parent.x, _parent.y, _parent.z);
		float dist = Vector3.Distance(jointPos, parentPos);
		if (_joint.distToParentMeasurements.Count == 0) {
			_joint.distToParent = dist;
			_joint.distToParentMeasurements.Add (dist);
		}
		else
			_joint.distToParent = replaceInAverage(_joint.distToParentMeasurements, _joint.distToParent, dist);
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

			jointData jd;
			//If there is a dictionary entry for the joint, update its position
			if (curr_skeleton_dict.TryGetValue (currJoint, out jd)) {
				curr_skeleton_dict.Remove (currJoint);
				jd.x = x;
				jd.y = y;
				jd.z = z;
				jd.trackState = trackState;
				curr_skeleton_dict.Add (currJoint, jd);
			}
			else
				curr_skeleton_dict.Add (currJoint, new jointData (x, y, z, trackState, getParentId(currJoint)));
		}

		//Update the joint positions if known, and colour them accordingly
		for (int i = 0; i <= 24; i++) 
		{
			//an empty joint data that we store our joint in if it exists
			jointData jd, jd_prev;


			//If there is a dictionary entry for the joint, update its position
			if (curr_skeleton_dict.TryGetValue (i, out jd) && prev_skeleton_dict.TryGetValue (i, out jd_prev)) 
			{
				Vector3 tracked_pos = new Vector3(jd.x, jd.y, jd.z);
				//Vector3 prev_pos = new Vector3(jd_prev.x, jd_prev.y, jd_prev.z);

				Vector3 final = tracked_pos;

				//float dist_moved = Vector3.Distance(tracked_pos, prev_pos);

				//If the position is known colour it blue
				if(jd.trackState == 2)
				{
					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_blue;

					jointData parentJoint;
					if (curr_skeleton_dict.TryGetValue (jd.parentID, out parentJoint))
						updateDistToParent(ref jd, parentJoint);

					curr_skeleton_dict.Remove (i);
					curr_skeleton_dict.Add (i, jd);

					final = kalman[i].Update(final);
				}
				//If the position is inferred colour it green
				else if(jd.trackState == 1)
				{
					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_green;

					jointData parentJoint, parentOfParentJoint;
					if(curr_skeleton_dict.TryGetValue(jd.parentID, out parentJoint))
					{
						if(curr_skeleton_dict.TryGetValue(parentJoint.parentID, out parentOfParentJoint))
						{
							Ray r = new Ray();
							r.origin = new Vector3(parentJoint.x, parentJoint.y, parentJoint.z);
							Vector3 parentPos = new Vector3(parentJoint.x, parentJoint.y, parentJoint.z);
							Vector3 parentOfParentPos = new Vector3(parentOfParentJoint.x, parentOfParentJoint.y, parentOfParentJoint.z);
							r.direction =  parentOfParentPos - parentPos;
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
			

		prev_skeleton_dict = new Dictionary<int, jointData>(curr_skeleton_dict);
	}

	public Dictionary<int, jointData> getSkeleDic()
	{
		Dictionary<int, jointData> ret = new Dictionary<int, jointData>(curr_skeleton_dict);
		return ret;
	}
}
