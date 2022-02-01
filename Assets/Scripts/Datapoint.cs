using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Datapoint : Terminal
{
	private int cursor_x;
	private int cursor_y;

	void Start()
	{
		Dial("its.pdp10.se", 10003);
		Clear(72, 25);
		cursor_x = 0;
		cursor_y = 0;
	}

	void Update()
	{
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
		case 11:
			cursor_y++;
			if (cursor_y >= 25)
				Scroll();
			break;
		case 13:
			cursor_x = 0;
			break;
		case 24:
			cursor_x++;
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
}
