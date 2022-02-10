using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Datapoint : Terminal
{
	private int cursor_x;
	private int cursor_y;

	void Start()
	{
		InitVRStuff();
		InitKeyboard();
		Dial(m_host, m_port);
		Clear(72, 25);
		cursor_x = 0;
		cursor_y = 0;
	}

	void Update()
	{
		UpdateKeyboard();
		Paint();
	}

	public override void Receive(byte data)
	{
		Debug.Log("Received character " + data);

		switch (data)
		{
		case 8:
		case 25:
			cursor_x--;
			break;
		case 9:
			cursor_x += 8;
			cursor_x &= -8;
			break;
		case 10:
			cursor_y++;
			if (cursor_y >= 25)
				Scroll();
			break;
		case 11:
			cursor_y++;
			break;
		case 13:
			cursor_x = 0;
			break;
		case 24:
			cursor_x++;
			break;
		case 26:
			cursor_y--;
			break;
		case 28:
			cursor_x = 0;
			cursor_y = 24;
			break;
		case 29:
			cursor_x = 0;
			cursor_y = 0;
			break;
		case 30:
			ClearLine(cursor_x, cursor_y);
			break;
		case 31:
			ClearScreen(cursor_x, cursor_y);
			break;
		default:
			if (data < 32 || data > 126)
				break;
			if (data == 94)
				data = 129;
			if (data == 95)
				data = 130;
			if (data >= 96 && data < 127)
				data -= 32;
			Print(cursor_x++, cursor_y, data);
			break;
		}

		if (cursor_x < 0)
			cursor_x = 0;
		if (cursor_x >= 72)
		{
			cursor_x = 0;
			cursor_y++;
			if (cursor_y >= 25)
				Scroll();
		}
		if (cursor_y >= 25)
			cursor_y = 24;
	}


	

	void InitKeyboard()
	{
		m_keys = new Key[54];
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
		m_keys[i++] = new Key(FindKey("key_BS"), 0x8, 0x8);
		m_keys[i++] = new Key(FindKey("key_HEREIS"), 0, 0);	// TODO

		m_keys[i++] = new Key(FindKey("key_ESC"), 0x1B, 0x1B);	// dunno about shift
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
		m_keys[i++] = new Key(FindKey("key_REPT"), 0, 0);	// TODO
		m_keys[i++] = new Key(FindKey("key_BREAK"), 0, 0);	// TODO, but may not even be possible

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
	}

	bool KeyDownL, SecKeyDownL;
	bool KeyDownR, SecKeyDownR;

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
						Send((byte)code);
					//	SendCode(code);
					}
					Debug.Log("Key down: " + ((char)code));
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

	void UpdateKeyboard()
	{
		KeyDownL = IsPressed(m_keyDownL.action);
		KeyDownR = IsPressed(m_keyDownR.action);
		SecKeyDownL = IsPressed(m_secKeyDownL.action);
		SecKeyDownR = IsPressed(m_secKeyDownR.action);
		for(int i = 0; i < m_keys.Length; i++)
			HandleKey(ref m_keys[i]);
	}
}
