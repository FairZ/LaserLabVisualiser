using UnityEngine;
using System.Collections;

public class HUDLerp : MonoBehaviour {
	private Vector3 m_lerpMin = new Vector3(0.1f,0.1f,0.1f);
	private Vector3 m_lerpMax = new Vector3(1,1,1);

	private float m_lerpTime=0.4f;
	private float m_currentLerpTime = 0.0f;

	private float m_countdownToHide = 0.0f;

	private bool m_toMax=false;
	private bool m_toMin=false;
	private bool m_hidden = true;
	private bool m_atMax = false;

	void Update(){
		if (m_countdownToHide <= 0.0f && !m_hidden) {
			m_toMin = true;
			m_atMax = false;
		} 
		else 
		{
			m_countdownToHide -= Time.deltaTime;
		}

		if (m_toMax == true) {
			m_currentLerpTime += Time.deltaTime;
			if (m_currentLerpTime > m_lerpTime) 
			{
				transform.localScale = m_lerpMax;
				m_toMax = false;
				m_atMax = true;
				m_currentLerpTime = 0.0f;
			}
			else
			{
				transform.localScale = Vector3.Lerp (m_lerpMin, m_lerpMax, m_currentLerpTime / m_lerpTime);
			}
		}
		if (m_toMin == true) {
			m_currentLerpTime += Time.deltaTime;
			if (m_currentLerpTime > m_lerpTime) 
			{
				transform.localScale = m_lerpMin;
				m_toMin = false;
				m_hidden = true;
				m_currentLerpTime = 0.0f;
			}
			else
			{
				transform.localScale = Vector3.Lerp (m_lerpMax, m_lerpMin, m_currentLerpTime / m_lerpTime);
			}
		}
	}

	public void LookedAt()
	{
		if (!m_atMax) 
		{
			if (m_toMin) 
			{
				m_currentLerpTime = m_lerpTime - m_currentLerpTime;
			}
			m_toMax = true;
			m_toMin = false;
			m_hidden = false;
		}
		m_countdownToHide = 1.0f;
	}

}
