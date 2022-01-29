using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DECtapeTransport : MonoBehaviour
{
	public GameObject m_networkManager;
	public GameObject m_object;
	public GameObject m_reelPrefab;
	public GameObject m_tapePrefab;
	public int m_unit;
	public int motion;

	GameObject m_leftReel;
	GameObject m_rightReel;
	GameObject m_unitKnob;
	GameObject m_modeSwitch;
	GameObject m_writeLock;

	Transform FindChildByName(Transform obj, string name)
	{
		if(obj.name == name)
			return obj;
		for(int i = 0; i < obj.childCount; i++) {
			Transform c = FindChildByName(obj.GetChild(i), name);
			if(c != null)
				return c;
		}
		return null;
	}

	void Start()
	{
		m_leftReel = FindChildByName(m_object.transform, "left_reel").gameObject;
		m_rightReel = FindChildByName(m_object.transform, "right_reel").gameObject;
		m_unitKnob = FindChildByName(m_object.transform, "unit_select").gameObject;
		m_modeSwitch = FindChildByName(m_object.transform, "mode_switch").gameObject;
		m_writeLock = FindChildByName(m_object.transform, "writelock_switch").gameObject;

		m_leftReel.transform.localEulerAngles = new Vector3(90.0f, 0.0f, Random.Range(0.0f, 360.0f));
		m_rightReel.transform.localEulerAngles = new Vector3(90.0f, 0.0f, Random.Range(0.0f, 360.0f));
		if(m_unit < 1 || m_unit > 8)
			m_unit = 4;

		GameObject reel1 = Instantiate(m_reelPrefab);
		reel1.transform.parent = m_leftReel.transform;
//		reel1.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		reel1.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		reel1.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.Range(0.0f, 360.0f));

		GameObject reel2 = Instantiate(m_reelPrefab);
		reel2.transform.parent = m_rightReel.transform;
//		reel2.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		reel2.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		reel2.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.Range(0.0f, 360.0f));

		// to connect the reels
		GameObject tape = Instantiate(m_tapePrefab);
		tape.transform.parent = m_object.transform;
		tape.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		tape.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
	}

	public float rotationSpeed = 4000.0f;

	void Update()
	{
		// unit 1: -10 deg, 8: 190 deg
		float unitAngle = (m_unit-1)*(200.0f/7.0f) - 10.0f;
		m_unitKnob.transform.localEulerAngles = new Vector3(90.0f, 0.0f, unitAngle);

		NetworkTest data = m_networkManager.GetComponent<NetworkTest>();
		int m = (data.dectape_motion>>((m_unit-1)*2))&3;
		motion = m&1;
		if((m&2)!=0) motion *= -1;

		m_leftReel.transform.Rotate(0.0f, 0.0f, motion*rotationSpeed*Time.deltaTime);
		m_rightReel.transform.Rotate(0.0f, 0.0f, motion*rotationSpeed*Time.deltaTime);
	}
}
