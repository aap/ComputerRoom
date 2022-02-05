using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public abstract class Terminal : MonoBehaviour
{
	public GameObject m_keyboard;
	public Texture2D m_texture;
	public Texture2D m_font;
	public int m_width;
	public int m_height;
	public int m_char_width;
	public int m_char_height;
	public int m_cell_width;
	public int m_cell_height;
	public byte[,] m_text;
	private bool updated = false;

	// IAC = 255
	// DO = 253
	// WILL = 251
	// IAC WILL 34 - linemode
	// IAC WILL 3 - suppress go ahead
	// IAC WILL 1 - echo
	// IAC WILL 0 - binary transmission
	// IAC DO 0

	TcpClient socket;
	NetworkStream stream;
	byte[] recvBuf = new byte[16];
	byte[] sendBuf = new byte[16];
	int pending = 0;

	public void Clear(int width, int height)
	{
		int x, y;
		Color black = new Color(0, 0, 0, 0xFF);
		for (y = 0; y < m_texture.height; y++)
			for (x = 0; x < m_texture.width; x++)
				m_texture.SetPixel(x, y, black);
		m_texture.Apply();

		m_width = width;
		m_height = height;
		m_text = new byte[m_width, m_height];
		updated = true;
	}

	public void Print(int x, int y, byte c)
	{
		m_text[x, y] = c;
		updated = true;
		x = m_cell_width*x + 2;
		y = m_cell_height*(m_height - 1 - y) + 2;
		Graphics.CopyTexture(m_font, 0, 0, 0,
			m_char_height*c, m_char_width, m_char_height,
			m_texture, 0, 0, x, y);
	}

	public void Scroll()
	{
		for (int y = 0; y < m_height - 1; y++)
			for (int x = 0; x < m_width; x++) {
				m_text[x, y] = m_text[x, y+1];
				Graphics.CopyTexture(m_font, 0, 0, 0,
				m_char_height*m_text[x, y],
				m_char_width, m_char_height,
				m_texture, 0, 0,
				m_cell_width*x + 2,
				m_cell_height*(m_height - 1 - y) + 2);
			}
		ClearLine(0, m_height - 1);
		updated = true;
	}

	public void ClearLine(int x, int y)
	{
		for (; x < m_width; x++) {
			m_text[x, y] = 32;
			Graphics.CopyTexture(m_font, 0, 0, 0,
			m_char_height*32, m_char_width, m_char_height,
			m_texture, 0, 0,
			m_cell_width*x + 2,
			m_cell_height*(m_height - 1 - y) + 2);
		}
		updated = true;
	}

	public void ClearScreen(int x, int y)
	{
		ClearLine (x, y);
		for (y++; y < m_height; y++)
			ClearLine (0, y);
		updated = true;
	}

	public void Paint()
	{
		if (pending > 0)
			Pending();

		if (updated)
			m_texture.Apply();
		updated = false;
	}

	public void Dial(String host, int port)
	{
		socket = new TcpClient {
			ReceiveBufferSize = 16,
			SendBufferSize = 16
		};
		Debug.Log("Connecting to " + host);
		socket.BeginConnect(host, port, ConnectCB, null);
		pending = 0;
	}

	private void ConnectCB(IAsyncResult result)
	{
		socket.EndConnect(result);
		if(!socket.Connected)
			return;

		Debug.Log("Connected.");
		stream = socket.GetStream();
		stream.BeginRead(recvBuf, 0, 16, ReceiveCB, null);
	}

	private void ReceiveCB(IAsyncResult result)
	{
		int n = stream.EndRead(result);
		if(n <= 0)
			return;

		pending = n;
		Debug.Log("Received: " + n);
	}

	private void Pending()
	{
		for(int i = 0; i < pending; i++)
			Receive(recvBuf[i]);
		pending = 0;
		stream.BeginRead(recvBuf, 0, 16, ReceiveCB, null);
	}

	public abstract void Receive(byte data);

	public void Send(byte data)
	{
		Debug.Log("Send: " + data);
		sendBuf[0] = data;
		stream.BeginWrite(sendBuf, 0, 1, SendCB, null);
	}

	private void SendCB(IAsyncResult result)
	{
		stream.EndWrite(result);
	}
}
