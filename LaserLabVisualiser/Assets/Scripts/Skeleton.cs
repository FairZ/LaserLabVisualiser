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

	Dictionary<int, JointData> prev_skeleton_dict;
	Dictionary<int, JointData> curr_skeleton_dict;

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

		for (int i = 0; i <= 24; i++) 
		{
			//an empty joint data that we store our joint in if it exists
			JointData jd;



			//If there is a dictionary entry for the joint, update its position
			if (curr_skeleton_dict.TryGetValue (i, out jd)) 
			{
				Vector3 final;
				Vector3 tracked_pos = jd.pos;
				//Vector3 prev_pos = new Vector3(jd_prev.x, jd_prev.y, jd_prev.z);


				//float dist_moved = Vector3.Distance(tracked_pos, prev_pos);

				if(jd.trackState == 2)
				{
					

					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_green;

					JointData parentJoint;
					//Update our average measurements with newly tracked data
					if (curr_skeleton_dict.TryGetValue (jd.parentID, out parentJoint)) 
					{
						jd.updateDistToParent (parentJoint);
						//Update based on parents, less preferable
						jd.updateDirParent (parentJoint);
						//Pull joint in if it is too far from parent
						if (Vector3.Distance (jd.pos, parentJoint.pos) > jd.distToParent * 1.05f ||
							Vector3.Distance (jd.pos, parentJoint.pos) < jd.distToParent * 0.95f) 
						{
							root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_blue;
							Ray r = new Ray ();
							r.origin = parentJoint.pos;
							r.direction = jd.pos - parentJoint.pos;
							tracked_pos = r.GetPoint (jd.distToParent);
						}
					}
					//Update direction based on last child direction if possible
					JointData childJoint;
					if (curr_skeleton_dict.TryGetValue (jd.getChildId(), out childJoint))
					{
						jd.updateDirChild (childJoint);
					} 

					curr_skeleton_dict.Remove (i);
					curr_skeleton_dict.Add (i, jd);
				}

				else if(jd.trackState == 1)
				{
					root.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material = dm_red;

					JointData parentJoint;
					if(curr_skeleton_dict.TryGetValue(jd.parentID, out parentJoint))
					{
							Ray r = new Ray();
							r.origin = parentJoint.pos;
						if (jd.isLimb ())
							r.direction = parentJoint.dir;
						else
							r.direction = jd.dir;
						
						tracked_pos = r.GetPoint(jd.distToParent);
					}
				}

				final = kalman[i].Update(tracked_pos);

				root.transform.GetChild (i).position = final*10 + new Vector3(12,0,0);//Scaled up to see clearer
			}
		}
			

		prev_skeleton_dict = new Dictionary<int, JointData>(curr_skeleton_dict);
	}

	public Dictionary<int, JointData> getSkeleDic()
	{
		Dictionary<int, JointData> ret = new Dictionary<int, JointData>(curr_skeleton_dict);
		return ret;
	}
}
