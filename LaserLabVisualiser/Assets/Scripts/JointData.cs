using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointData {

	public Vector3 pos;
	public Vector3 dir;

	public float distToParent;
	public List<float> distToParentMeasurements;

	public int trackState;
	public int id, parentID, childID;

	uint avPrecision = 5;

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

	public JointData(float _x, float _y, float _z, int _trackState, int _id)
	{
		pos = new Vector3 (_x, _y, _z);

		distToParent = 0.0f;

		distToParentMeasurements = new List<float>();

		trackState = _trackState;
		id = _id;
		parentID = getParentId();
		childID = getChildId();
	}


	public int getParentId()
	{
		return getParentId (id);
	}

	//https://vvvv.org/sites/default/files/images/kinectskeleton-map2.png
	public int getParentId(int _id)
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

	public int getChildId()
	{
		return getChildId (id);
	}

	public int getChildId(int _id)
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

	public bool isLimb()
	{
		if ((JointType)id == JointType.ShoulderLeft || (JointType)id == JointType.ShoulderLeft)
			return false;
		else if (getChildId(getParentId ()) < 0)
			return false;
		else
			return true;
	}

	float replaceInAverage(float _newValue)
	{
		float ret = (distToParentMeasurements.Count * distToParent - distToParentMeasurements[0] + _newValue) / distToParentMeasurements.Count;

		distToParentMeasurements.Add(_newValue);

		distToParentMeasurements.RemoveAt(0);

		return ret;
	}

	public void updateDistToParent(JointData _parent)
	{
		//Update distance to parent
		Vector3 parentPos = _parent.pos;
		float dist = Vector3.Distance(pos, parentPos);
		if (distToParentMeasurements.Count <= avPrecision)
		{
			distToParent = dist;
			distToParentMeasurements.Add (dist);
		}
		else
			distToParent = replaceInAverage(dist);
	}

	public void updateDirChild(JointData _child)
	{
		//Update Direction
		dir =  _child.pos - pos;
	}

	public void updateDirParent(JointData _parent)
	{
		//Update Direction
		dir = pos - _parent.pos;
	}
}
