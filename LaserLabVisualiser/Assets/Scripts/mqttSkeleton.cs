using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class mqttSkeleton : mqttClient 
{
	

	public List<Material> debugColours;

	String[] localData;
	int lineCount;
	private Skeleton activeSkeleton;
	private cleanSkeleton cleanSkeleton;

	void createBallSkeleton()
	{
		//Create a sphere to use as a template
		GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		sphere.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);

		//Add line for drawing a stickman
		sphere.AddComponent<LineRenderer>();
		sphere.GetComponent<LineRenderer>().material = debugColours[3];
		sphere.GetComponent<LineRenderer>().widthMultiplier = 0.2f;
		sphere.GetComponent<LineRenderer>().receiveShadows = false;
		//Create a gameobject to attach the joints to
		GameObject parent = new GameObject("Skeleton");

		List<Transform> joints = new List<Transform>();
		//24 is length of joint types
		//Each iteration make a copy of the template sphere and attach it to the parent and name it
		for (uint i = 0; i < 24; i++) {
			GameObject tmpGO = Instantiate (sphere);
			//Cast to our enum to determine name
			JointData.JointType jt = (JointData.JointType)i;
			tmpGO.name = jt.ToString ();
			tmpGO.transform.SetParent (parent.transform);
			joints.Add(tmpGO.transform);

		}
		//Finally set the template to be the final joint, more efficient
		JointData.JointType _jt = (JointData.JointType)24;
		sphere.name = _jt.ToString ();
		sphere.transform.SetParent (parent.transform);
		//Set the active skeleton to be our parent
		activeSkeleton = new Skeleton(parent, debugColours);
	}

	void createCleanSkeleton()
	{
		//Create a sphere to use as a template
		GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		sphere.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);

		//Add line for drawing a stickman
		sphere.AddComponent<LineRenderer>();
		sphere.GetComponent<LineRenderer>().material = debugColours[3];
		sphere.GetComponent<LineRenderer>().widthMultiplier = 0.2f;
		sphere.GetComponent<LineRenderer>().receiveShadows = false;
		//Create a gameobject to attach the joints to
		GameObject parent = new GameObject("Skeleton");

		List<Transform> joints = new List<Transform>();
		//24 is length of joint types
		//Each iteration make a copy of the template sphere and attach it to the parent and name it
		for (uint i = 0; i < 24; i++) {
			GameObject tmpGO = Instantiate (sphere);
			//Cast to our enum to determine name
			JointData.JointType jt = (JointData.JointType)i;
			tmpGO.name = jt.ToString ();
			tmpGO.transform.SetParent (parent.transform);
			joints.Add(tmpGO.transform);

		}
		//Finally set the template to be the final joint, more efficient
		JointData.JointType _jt = (JointData.JointType)24;
		sphere.name = _jt.ToString ();
		sphere.transform.SetParent (parent.transform);
		//Set the active skeleton to be our parent
		cleanSkeleton = new cleanSkeleton(parent, debugColours);
	}

	//Imperfect but functional, relatable function really...
	IEnumerator fileText()
	{
		while (Application.isPlaying) {
			m_data = localData [lineCount];
			if (lineCount == localData.Length-1)
				lineCount = 0;
			lineCount++;
			yield return new WaitForSeconds (0.05f);
		}
	}

	void fromFile()
	{
		if (File.Exists (filePath)) {
			string tmp_data = File.ReadAllText (filePath);
			localData = tmp_data.Split ('*');
			StartCoroutine (fileText ());
		}
		else
		{
			Debug.LogError ("File not found/accessible at: " + filePath);
		}
	}

	void updateLinesPos()
	{
		for(int i = 1; i <= 24; i++)
		{
			JointData jd;
			if(activeSkeleton.getSkeleDic().TryGetValue(i, out jd))
			{
				activeSkeleton.root.transform.GetChild(i).gameObject.GetComponent<LineRenderer>().SetPosition(0, activeSkeleton.root.transform.GetChild(i).position);
				if(jd.parentID >= 0)
					activeSkeleton.root.transform.GetChild(i).gameObject.GetComponent<LineRenderer>().SetPosition(1, activeSkeleton.root.transform.GetChild(jd.parentID).position);
			}	
			if(cleanSkeleton.getSkeleDic().TryGetValue(i, out jd))
			{
				cleanSkeleton.root.transform.GetChild(i).gameObject.GetComponent<LineRenderer>().SetPosition(0, cleanSkeleton.root.transform.GetChild(i).position);
				if(jd.parentID >= 0)
					cleanSkeleton.root.transform.GetChild(i).gameObject.GetComponent<LineRenderer>().SetPosition(1, cleanSkeleton.root.transform.GetChild(jd.parentID).position);
			}	
		}
	}

	void Awake()
	{
		createBallSkeleton();
		createCleanSkeleton ();
		if (offline)
			fromFile ();
	}


	// Update is called once per frame
	void Update () 
	{
		if(m_data != "")
		{
			activeSkeleton.updateJoints(m_data);
			cleanSkeleton.updateJoints (m_data);
			updateLinesPos();
		}
	}
}
