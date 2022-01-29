using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PDP6_Panel : MonoBehaviour
{
	public GameObject m_leftHand, m_rightHand;
	public InputActionProperty m_swDownL, m_swDownR, m_swUpL, m_swUpR;
	public GameObject m_pdp6_console;
	public Material m_lampBaseMat;
	public GameObject m_networkManager;

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

	Material GetLamp(string name)
	{
		Material mat = new Material(m_lampBaseMat);

		Transform lamp = FindChildByName(m_pdp6_console.transform, name);
		MeshRenderer renderer = lamp.gameObject.GetComponent<MeshRenderer>();
		Material[] mats = renderer.materials;
		mats[1] = mat;
		renderer.materials = mats;
		return mat;
	}

	GameObject GetSwitch(string name)
	{
		GameObject obj = FindChildByName(m_pdp6_console.transform, name).gameObject;
		obj.AddComponent<SphereCollider>();
		XRSimpleInteractable intact = obj.AddComponent<XRSimpleInteractable>();
		return obj;
	}

	Material[] m_lampsIR;
	Material[] m_lampsMI;
	Material[] m_lampsPC;
	Material[] m_lampsMA;
	Material[] m_lampsPIH;
	Material[] m_lampsPIR;
	Material[] m_lampsPIO;
	Material m_lampRun;
	Material m_lampMemstop;
	Material m_lampPiOn;
	Material m_lampAddrStop;
	Material m_lampRepeat;
	Material m_lampDisMem;
	Material m_lampPower;

	Material[] m_lampsMB;
	Material[] m_lampsAR;
	Material[] m_lampsMQ;
	Material[] m_lampsFlags;

	GameObject[] m_swData;
	GameObject[] m_swAddr;
	GameObject m_swAddrStop;
	GameObject m_swRepeat;
	GameObject m_swDisMem;
	GameObject m_swPower;
	GameObject m_keyStart;
	GameObject m_keyCont;
	GameObject m_keyStop;
	GameObject m_keyReset;
	GameObject m_keyDeposit;
	GameObject m_keyExamine;
	GameObject m_keyReader;
	GameObject m_keyFeed;

	public int[] state_sw_data;
	public int[] state_sw_addr;
	public int state_sw_addrstop;
	public int state_sw_repeat;
	public int state_sw_dismem;
	public int state_sw_power;
	public int state_key_start;
	public int state_key_cont;
	public int state_key_stop;
	public int state_key_reset;
	public int state_key_deposit;
	public int state_key_examine;
	public int state_key_reader;
	public int state_key_feed;

	XRRayInteractor leftHover;
	XRRayInteractor rightHover;

	void Start()
	{
		leftHover = m_leftHand.GetComponent<XRRayInteractor>();
		rightHover = m_rightHand.GetComponent<XRRayInteractor>();

		state_sw_data = new int[36];
		state_sw_addr = new int[18];
		state_sw_addrstop = 0;
		state_sw_repeat = 0;
		state_sw_dismem = 0;
		state_sw_power = 0;
		state_key_start = 0;
		state_key_cont = 0;
		state_key_stop = 0;
		state_key_reset = 0;
		state_key_deposit = 0;
		state_key_examine = 0;
		state_key_reader = 0;
		state_key_feed = 0;

		m_lampsIR = new Material[18];
		m_lampsMI = new Material[36];
		m_lampsPC = new Material[18];
		m_lampsMA = new Material[18];
		m_lampsPIH = new Material[7];
		m_lampsPIR = new Material[7];
		m_lampsPIO = new Material[7];
		m_lampsMB = new Material[36];
		m_lampsAR = new Material[36];
		m_lampsMQ = new Material[36];
		m_lampsFlags = new Material[14*8];
		for(int i = 0; i < 18; i++) {
			m_lampsIR[i] = GetLamp("lamp_IR" + i.ToString("00"));
			m_lampsPC[i] = GetLamp("lamp_PC" + (i+18).ToString("00"));
			m_lampsMA[i] = GetLamp("lamp_MA" + (i+18).ToString("00"));
		}
		for(int i = 0; i < 36; i++) {
			m_lampsMI[i] = GetLamp("lamp_MI" + i.ToString("00"));
			m_lampsMB[i] = GetLamp("lamp_MB" + i.ToString("00"));
			m_lampsAR[i] = GetLamp("lamp_AR" + i.ToString("00"));
			m_lampsMQ[i] = GetLamp("lamp_MQ" + i.ToString("00"));
		}
		for(int i = 0; i < 7; i++) {
			m_lampsPIH[i] = GetLamp("lamp_PIH" + (i+1));
			m_lampsPIR[i] = GetLamp("lamp_PIR" + (i+1));
			m_lampsPIO[i] = GetLamp("lamp_PIO" + (i+1));
		}
		for(int i = 0; i < 14; i++)
			for(int j = 0; j < 8; j++)
				m_lampsFlags[i*8 + j] = GetLamp("lamp_flags" + i.ToString("00") + "_" + j.ToString("0"));
		m_lampRun = GetLamp("lamp_run");
		m_lampMemstop = GetLamp("lamp_memstop");
		m_lampPiOn = GetLamp("lamp_pion");
		m_lampAddrStop = GetLamp("lamp_addrstop");
		m_lampRepeat = GetLamp("lamp_repeat");
		m_lampDisMem = GetLamp("lamp_dismem");
		m_lampPower = GetLamp("lamp_power");

		m_swData = new GameObject[36];
		m_swAddr = new GameObject[18];
		for(int i = 0; i < 18; i++)
			m_swAddr[i] = GetSwitch("sw_addr" + i.ToString("00"));
		for(int i = 0; i < 36; i++)
			m_swData[i] = GetSwitch("sw_data" + i.ToString("00"));
		m_swAddrStop = GetSwitch("sw_addrstop");
		m_swRepeat = GetSwitch("sw_repeat");
		m_swDisMem = GetSwitch("sw_dismem");
		m_swPower = GetSwitch("sw_power");

		m_keyStart = GetSwitch("key_start");
		m_keyCont = GetSwitch("key_cont");
		m_keyStop = GetSwitch("key_stop");
		m_keyReset = GetSwitch("key_reset");
		m_keyDeposit = GetSwitch("key_deposit");
		m_keyExamine = GetSwitch("key_examine");
		m_keyReader = GetSwitch("key_reader");
		m_keyFeed = GetSwitch("key_feed");
	}

	Vector3 swRotDown = new Vector3(115.0f, 0.0f, 0.0f);
	Vector3 swRotUp = new Vector3(50.0f, 0.0f, 0.0f);
	bool switchDownL;
	bool switchDownR;
	bool switchUpL;
	bool switchUpR;

	void HandleSwitch(ref int state, GameObject obj)
	{
		XRBaseInteractable intact = obj.GetComponent<XRBaseInteractable>();
		int stateL = 0;
		int stateR = 0;
		if(intact.interactorsHovering.Contains(leftHover)) {
			if(switchDownL) stateL = -1;
			else if(switchUpL) stateL = 1;
		} else if(intact.interactorsHovering.Contains(rightHover)) {
			if(switchDownR) stateR = -1;
			else if(switchUpR) stateR = 1;
		}
		state = Math.Clamp(state + stateL+stateR, 0, 1);
		obj.transform.localEulerAngles = state!=0 ? swRotUp : swRotDown;
	}

	void HandleKey(ref int state, GameObject obj)
	{
		XRBaseInteractable intact = obj.GetComponent<XRBaseInteractable>();
		float delta = 32.0f;
		Vector3 angles = new Vector3(0.0f, 90.0f, 83.0f);
		int stateL = 0;
		int stateR = 0;
		if(intact.interactorsHovering.Contains(leftHover)) {
			if(switchDownL) stateL = 1;
			else if(switchUpL) stateL = -1;
		} else if(intact.interactorsHovering.Contains(rightHover)) {
			if(switchDownR) stateR = 1;
			else if(switchUpR) stateR = -1;
		}
		state = Math.Clamp(stateL+stateR, -1, 1);
		angles.z += state*delta;
		obj.transform.localEulerAngles = angles;
	}

	void Update()
	{
		Color on = new Color(1.0f, 0.7576219f, 0.2216981f);
		Color off = new Color(0.65f, 0.65f, 0.65f);

		NetworkTest data = m_networkManager.GetComponent<NetworkTest>();
		int bit = 1<<17;
		for(int i = 0; i < 18; i++){
			m_lampsIR[i].color = (data.l_ir & bit)!=0 ? on : off;
			m_lampsPC[i].color = (data.l_pc & bit)!=0 ? on : off;
			m_lampsMA[i].color = (data.l_ma & bit)!=0 ? on : off;

			m_lampsMI[i].color = (data.l_milt & bit)!=0 ? on : off;
			m_lampsMI[i+18].color = (data.l_mirt & bit)!=0 ? on : off;
			m_lampsMB[i].color = (data.l_mblt & bit)!=0 ? on : off;
			m_lampsMB[i+18].color = (data.l_mbrt & bit)!=0 ? on : off;
			m_lampsAR[i].color = (data.l_arlt & bit)!=0 ? on : off;
			m_lampsAR[i+18].color = (data.l_arrt & bit)!=0 ? on : off;
			m_lampsMQ[i].color = (data.l_mqlt & bit)!=0 ? on : off;
			m_lampsMQ[i+18].color = (data.l_mqrt & bit)!=0 ? on : off;

			bit >>= 1;
		}
		bit = 1<<6;
		for(int i = 0; i < 7; i++){
			m_lampsPIH[i].color = (data.l_pih & bit)!=0 ? on : off;
			m_lampsPIR[i].color = (data.l_pir & bit)!=0 ? on : off;
			m_lampsPIO[i].color = (data.l_pio & bit)!=0 ? on : off;
			bit >>= 1;
		}
		m_lampRun.color = data.l_run!=0 ? on : off;
		m_lampMemstop.color = data.l_memstop!=0 ? on : off;
		m_lampPiOn.color = data.l_pion!=0 ? on : off;
		m_lampAddrStop.color = state_sw_addrstop!=0 ? on : off;
		m_lampRepeat.color = state_sw_repeat!=0 ? on : off;
		m_lampDisMem.color = state_sw_dismem!=0 ? on : off;
		m_lampPower.color = state_sw_power!=0 ? on : off;

		for(int i = 0; i < 14; i++) {
			bit = 1<<7;
			for(int j = 0; j < 8; j++) {
				m_lampsFlags[i*8 + j].color = (data.l_flags[i] & bit) !=0 ? on : off;
				bit >>= 1;
			}
		}

		switchDownL = IsPressed(m_swDownL.action);
		switchDownR = IsPressed(m_swDownR.action);
		switchUpL = IsPressed(m_swUpL.action);
		switchUpR = IsPressed(m_swUpR.action);
		for(int i = 0; i < 36; i++)
			HandleSwitch(ref state_sw_data[i], m_swData[i]);
		for(int i = 0; i < 18; i++)
			HandleSwitch(ref state_sw_addr[i], m_swAddr[i]);
		HandleSwitch(ref state_sw_addrstop, m_swAddrStop);
		HandleSwitch(ref state_sw_repeat, m_swRepeat);
		HandleSwitch(ref state_sw_dismem, m_swDisMem);
		HandleSwitch(ref state_sw_power, m_swPower);

		HandleKey(ref state_key_start, m_keyStart);
		HandleKey(ref state_key_cont, m_keyCont);
		HandleKey(ref state_key_stop, m_keyStop);
		HandleKey(ref state_key_reset, m_keyReset);
		HandleKey(ref state_key_deposit, m_keyDeposit);
		HandleKey(ref state_key_examine, m_keyExamine);
		HandleKey(ref state_key_reader, m_keyReader);
		HandleKey(ref state_key_feed, m_keyFeed);

		data.key_start = state_key_start > 0 ?1:0;
		data.key_readin = state_key_start < 0 ?1:0;
		data.key_inst_cont = state_key_cont > 0 ?1:0;
		data.key_mem_cont = state_key_cont < 0 ?1:0;
		data.key_inst_stop = state_key_stop > 0 ?1:0;
		data.key_mem_stop = state_key_stop < 0 ?1:0;
		data.key_io_reset = state_key_reset > 0 ?1:0;
		data.key_exec = state_key_reset < 0 ?1:0;
		data.key_dep = state_key_deposit > 0 ?1:0;
		data.key_dep_nxt = state_key_deposit < 0 ?1:0;
		data.key_ex = state_key_examine > 0 ?1:0;
		data.key_ex_nxt = state_key_examine < 0 ?1:0;
		data.key_rd_off = state_key_reader > 0 ?1:0;
		data.key_rd_on = state_key_reader < 0 ?1:0;
		data.key_pt_rd = state_key_feed > 0 ?1:0;
		data.key_pt_wr = state_key_feed < 0 ?1:0;

		data.sw_addr_stop = state_sw_addrstop;
		data.sw_repeat = state_sw_repeat;
		data.sw_mem_disable = state_sw_dismem;
		data.sw_power = state_sw_power;

		int t;
		t = 0;
		for(int i = 0; i < 18; i++)
			t = t<<1 | state_sw_data[i];
		data.sw_dataLT = t;
		t = 0;
		for(int i = 0; i < 18; i++)
			t = t<<1 | state_sw_data[i+18];
		data.sw_dataRT = t;
		t = 0;
		for(int i = 0; i < 18; i++)
			t = t<<1 | state_sw_addr[i];
		data.sw_mas = t;

	}

/*
	public void HoverEnter(HoverEnterEventArgs args)
	{
//		Debug.Log("Entering " + obj.name);
		Debug.Log("Entering");
	}
	public void HoverLeave(XRBaseInteractable obj)
	{
		Debug.Log("Leaving " + obj.name);
	}
*/

	// taken from ActionBasedController
	float m_ButtonPressPoint = 0.5f;
        protected bool IsPressed(InputAction action)
        {
            if (action == null)
                return false;

#if INPUT_SYSTEM_1_1_OR_NEWER
                return action.phase == InputActionPhase.Performed;
#else
            if (action.activeControl is ButtonControl buttonControl)
                return buttonControl.isPressed;

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>() >= m_ButtonPressPoint;

            return action.triggered || action.phase == InputActionPhase.Performed;
#endif
        }
}
