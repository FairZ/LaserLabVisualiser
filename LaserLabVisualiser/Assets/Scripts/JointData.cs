using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointData {

	public Vector3 pos;
	public Vector3 dir;

	public float distToParent;
	public List<float> distToParentMeasurements;

	public int trackState;
	public int parentID;

	uint avPrecision = 5;

	public JointData(float _x, float _y, float _z, int _trackState, int _parentID)
	{
		pos = new Vector3 (_x, _y, _z);

		distToParent = 0.0f;

		distToParentMeasurements = new List<float>();

		trackState = _trackState;
		parentID = _parentID;
	}

	float replaceInAverage(float _newValue)
	{
		float ret = _newValue;
		if(distToParentMeasurements.Count > 0)
			ret = (distToParentMeasurements.Count * distToParent - distToParentMeasurements[0] + _newValue) / distToParentMeasurements.Count;

		distToParentMeasurements.Add(_newValue);

		distToParentMeasurements.RemoveAt(0);

		return ret;
	}

	public void updateDistToParent(JointData _parent)
	{
		//Update distance to parent
		Vector3 parentPos = _parent.pos;
		float dist = Vector3.Distance(pos, parentPos);
		if (distToParentMeasurements.Count <= avPrecision) {
			distToParent = dist;
			distToParentMeasurements.Add (dist);
		}
		else
			distToParent = replaceInAverage(dist);

		//Update Direction
		dir = pos - _parent.pos;
	}

	public void updateDirChild(JointData _child)
	{
		//Update Direction
		dir = pos - _child.pos;
	}

	public void updateDirParent(JointData _parent)
	{
		//Update Direction
		dir = pos - _parent.pos;
	}
}
