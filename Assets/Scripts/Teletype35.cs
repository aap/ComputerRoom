using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;
using System.Net;
using System.Net.Sockets;

public class Teletype35 : MonoBehaviour
{
	public GameObject m_leftHand, m_rightHand;
	public InputActionProperty m_keyDownL, m_keyDownR;
	public InputActionProperty m_secKeyDownL, m_secKeyDownR;
	public GameObject m_keyboard;
	public GameObject m_textObj;
	string m_text;
	char[] m_line = new char[73];
	int m_cursor;

	struct Key {
		public GameObject key;
		public int ascii, ascii_shift;
		public int state;
		public Key(GameObject k, int a, int ash) { key = k; ascii = a; ascii_shift = ash; state = 0; }
	};
	Key[] m_keys;

	bool ShiftDown;
	bool CtrlDown;

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

	GameObject FindKey(string name)
	{
		GameObject obj = FindChildByName(m_keyboard.transform, name).gameObject;
		// what a terrible hack:
		// create parent just for box collider so it stays the same when the key is down
		GameObject collobj = new GameObject(obj.name + "_coll");
		collobj.transform.position = obj.transform.position;
		collobj.transform.rotation = obj.transform.rotation;
		collobj.transform.parent = obj.transform.parent;
		obj.transform.parent = collobj.transform;
		// and now it gets worse: create it on the child and copy so the dimensions are good
		BoxCollider t = obj.AddComponent<BoxCollider>();
		BoxCollider col = collobj.AddComponent<BoxCollider>();
		// have to decrease height so we don't accidentally press other keys
		col.center = t.center;
		col.size = t.size;
		Destroy(t);
		float h = col.size.z/4.0f;
		Vector3 v = col.center;
		v.z += (col.size.z-h)/2.0f;
		col.center = v;
		v = col.size;
		v.z = h;
		col.size = v;

		XRSimpleInteractable intact = collobj.AddComponent<XRSimpleInteractable>();
		return collobj;
	}

	XRRayInteractor leftHover;
	XRRayInteractor rightHover;
	void Start()
	{
		leftHover = m_leftHand.GetComponent<XRRayInteractor>();
		rightHover = m_rightHand.GetComponent<XRRayInteractor>();

		m_textObj = FindChildByName(transform, "Text").gameObject;

		for(int j = 0; j < m_line.Length; j++) m_line[j] = ' ';
		m_line[m_line.Length-1] = '\n';
		m_textObj.GetComponent<TMPro.TextMeshPro>().text = "";

		m_keys = new Key[50];
		int i = 0;
		m_keys[i++] = new Key(FindKey("key_1"), 0x31, 0x21);
		m_keys[i++] = new Key(FindKey("key_2"), 0x32, 0x22);
		m_keys[i++] = new Key(FindKey("key_3"), 0x33, 0x23);
		m_keys[i++] = new Key(FindKey("key_4"), 0x34, 0x24);
		m_keys[i++] = new Key(FindKey("key_5"), 0x35, 0x25);
		m_keys[i++] = new Key(FindKey("key_6"), 0x36, 0x26);
		m_keys[i++] = new Key(FindKey("key_7"), 0x37, 0x27);
		m_keys[i++] = new Key(FindKey("key_8"), 0x38, 0x28);
		m_keys[i++] = new Key(FindKey("key_9"), 0x39, 0x29);
		m_keys[i++] = new Key(FindKey("key_0"), 0x30, 0x30);	// shifted probably wrong
		m_keys[i++] = new Key(FindKey("key_colon"), 0x3A, 0x2A);
		m_keys[i++] = new Key(FindKey("key_minus"), 0x2D, 0x3D);

		m_keys[i++] = new Key(FindKey("key_ALT"), 0x7D, 0x7D);	// dunno about shift
		m_keys[i++] = new Key(FindKey("key_q"), 0x51, -3);
		m_keys[i++] = new Key(FindKey("key_w"), 0x57, -3);
		m_keys[i++] = new Key(FindKey("key_e"), 0x45, -3);
		m_keys[i++] = new Key(FindKey("key_r"), 0x52, -3);
		m_keys[i++] = new Key(FindKey("key_t"), 0x54, -3);
		m_keys[i++] = new Key(FindKey("key_y"), 0x59, -3);
		m_keys[i++] = new Key(FindKey("key_u"), 0x55, -3);
		m_keys[i++] = new Key(FindKey("key_i"), 0x49, -3);
		m_keys[i++] = new Key(FindKey("key_o"), 0x4F, 0x5F);	// _
		m_keys[i++] = new Key(FindKey("key_p"), 0x50, 0x40);	// @
		m_keys[i++] = new Key(FindKey("key_LF"), 0x0A, 0x0A);	// dunno about shift
		m_keys[i++] = new Key(FindKey("key_CR"), 0x0D, 0x0D);	// dunno about shift

		m_keys[i++] = new Key(FindKey("key_CTRL"), -1, -1);
		m_keys[i++] = new Key(FindKey("key_a"), 0x41, -3);
		m_keys[i++] = new Key(FindKey("key_s"), 0x53, -3);
		m_keys[i++] = new Key(FindKey("key_d"), 0x44, -3);
		m_keys[i++] = new Key(FindKey("key_f"), 0x46, -3);
		m_keys[i++] = new Key(FindKey("key_g"), 0x47, -3);
		m_keys[i++] = new Key(FindKey("key_h"), 0x48, -3);
		m_keys[i++] = new Key(FindKey("key_j"), 0x4A, -3);
		m_keys[i++] = new Key(FindKey("key_k"), 0x4B, 0x5B);	// [
		m_keys[i++] = new Key(FindKey("key_l"), 0x4C, 0x5C);	// \
		m_keys[i++] = new Key(FindKey("key_semi"), 0x3B, 0x2B);
		m_keys[i++] = new Key(FindKey("key_RUBOUT"), 0x7F, 0x7F);	// dunno about shift

		m_keys[i++] = new Key(FindKey("key_LSHIFT"), -2, -2);
		m_keys[i++] = new Key(FindKey("key_z"), 0x5A, -3);
		m_keys[i++] = new Key(FindKey("key_x"), 0x58, -3);
		m_keys[i++] = new Key(FindKey("key_c"), 0x43, -3);
		m_keys[i++] = new Key(FindKey("key_v"), 0x56, -3);
		m_keys[i++] = new Key(FindKey("key_b"), 0x42, -3);
		m_keys[i++] = new Key(FindKey("key_n"), 0x4E, 0x5E);	// ^
		m_keys[i++] = new Key(FindKey("key_m"), 0x4D, 0x5D);	// ]
		m_keys[i++] = new Key(FindKey("key_comma"), 0x2C, 0x3C);
		m_keys[i++] = new Key(FindKey("key_period"), 0x2E, 0x3E);
		m_keys[i++] = new Key(FindKey("key_slash"), 0x2F, 0x3F);
		m_keys[i++] = new Key(FindKey("key_RSHIFT"), -2, -2);

		m_keys[i++] = new Key(FindKey("key_space"), 0x20, 0x20);	// dunno about shift



		socket = new TcpClient {
			ReceiveBufferSize = 16,
			SendBufferSize = 16
		};
		socket.BeginConnect("soma", 2001, ConnectCB, null);
	}

	bool KeyDownL, SecKeyDownL;
	bool KeyDownR, SecKeyDownR;

	void UpdateText() {
		m_textObj.GetComponent<TMPro.TextMeshPro>().text = m_text + new string(m_line);
	}

	void NewLine()
	{
		m_text += new string(m_line);
		for(int i = 0; i < m_line.Length; i++) m_line[i] = ' ';
		m_line[m_line.Length-1] = '\n';
	}

	void RecvCode(int code)
	{
		code &= 0x7F;
		if(code == 0xA)
			NewLine();
		else if(code == 0xD)
			m_cursor = 0;
		else if(code == 0x8)
			m_cursor--;
		else if(code == 0x9)
			m_cursor = (m_cursor+7)%8;
		else if(code >= 0x20 && code < 0x60)
			m_line[m_cursor++] = (char)code;
		else
			Debug.Log("not handling code " + code);

		if(m_cursor < 0) m_cursor = 0;
		if(m_cursor >= m_line.Length-1) m_cursor = m_line.Length-2;
		UpdateText();
	}

	void SendCode(int code)
	{
		if(socket.Connected)
			SendNetwork((byte)code);
		else
			RecvCode(code);	// loopback for testing
	}

	void HandleKey(ref Key k)
	{
		XRBaseInteractable intact = k.key.GetComponent<XRBaseInteractable>();
		if(intact.interactorsHovering.Contains(leftHover) && (KeyDownL || SecKeyDownL) ||
		   intact.interactorsHovering.Contains(rightHover) && (KeyDownR || SecKeyDownR)) {
			if(k.state == 0) {	// was up
				if(k.ascii == -1) CtrlDown = true;
				else if(k.ascii == -2) ShiftDown = true;
				else {
					// TODO? may want to only do this for the hovering hand
					bool shiftHack = SecKeyDownL || SecKeyDownR;
					int code = (ShiftDown || shiftHack) ? k.ascii_shift : k.ascii;
					if(code >= 0) {
						if(CtrlDown) code &= 0x1F;
						SendCode(code);
					}
				//	Debug.Log("Key down: " + ((char)code));
				}
				k.key.transform.GetChild(0).Translate(0.0f, 0.0f, -0.008f);
				k.state = 1;
			}
		} else {
			if(k.state == 1) {	// was down
				// BUG: doesn't work if both shifts are pressed
				if(k.ascii == -1) CtrlDown = false;
				else if(k.ascii == -2) ShiftDown = false;
				k.key.transform.GetChild(0).Translate(0.0f, 0.0f, 0.008f);
				k.state = 0;
			}
		}
	}

	void Update()
	{
		KeyDownL = IsPressed(m_keyDownL.action);
		KeyDownR = IsPressed(m_keyDownR.action);
		SecKeyDownL = IsPressed(m_secKeyDownL.action);
		SecKeyDownR = IsPressed(m_secKeyDownR.action);
		for(int i = 0; i < m_keys.Length; i++)
			HandleKey(ref m_keys[i]);

		DrainBuffer();
	}



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




	/* the network part */
	TcpClient socket;
	NetworkStream stream;
	byte[] recvBuf = new byte[16];
	byte[] sendBuf = new byte[16];
	int nBytesRead;

	private void ConnectCB(IAsyncResult result)
	{
		socket.EndConnect(result);
		if(!socket.Connected)
			return;

		stream = socket.GetStream();

		stream.BeginRead(recvBuf, 0, 1, ReceiveCB, null);
	}

	private void ReceiveCB(IAsyncResult result)
	{
		int n = stream.EndRead(result);
		if(n <= 0)
			return;

		nBytesRead = n;

//		for(int i = 0; i < n; i++)
//			RecvCode(recvBuf[i]);

//	Debug.Log("Start receiving");
//		stream.BeginRead(recvBuf, 0, 16, ReceiveCB, null);
	}

	private void DrainBuffer()
	{
		if(nBytesRead > 0) {
			for(int i = 0; i < nBytesRead; i++)
				RecvCode(recvBuf[i]);
			nBytesRead = 0;
			// read new buffer
			stream.BeginRead(recvBuf, 0, 1, ReceiveCB, null);
		}
	}

	private void SendNetwork(byte code)
	{
		code |= 0x80;
		sendBuf[0] = code;
		stream.BeginWrite(sendBuf, 0, 1, SendCB, null);
	}

	private void SendCB(IAsyncResult result)
	{
		stream.EndWrite(result);
	}
}
