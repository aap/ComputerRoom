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
	public int m_width;
	public int m_height;
	public char[,] m_text;
	private bool updated = false;

	TcpClient socket;
	NetworkStream stream;
	byte[] recvBuf = new byte[16];
	byte[] sendBuf = new byte[16];
	int nBytesRead;

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
		m_text = new char[m_width, m_height];
		updated = true;
	}

	public void Print(int x, int y, char c)
	{
		m_text[x, y] = c;
		updated = true;

		Color green = new Color(0x00, 0xFF, 0x00, 0xFF);
		x = UnityEngine.Random.Range(0, m_texture.width);
		y = UnityEngine.Random.Range(0, m_texture.height);
		m_texture.SetPixel(x, y, green);
	}

	public void Scroll()
        {
		for (int y = 0; y < m_height - 1; y++)
			for (int x = 0; x < m_width; x++)
				m_text[x, y] = m_text[x, y+1];
		updated = true;
        }

	public void ClearLine(int x, int y)
	{
		for (; x < m_width; x++)
			m_text[x, y] = ' ';
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
	}

	private void ConnectCB(IAsyncResult result)
	{
		socket.EndConnect(result);
		if(!socket.Connected)
			return;

		Debug.Log("Connected.");
		stream = socket.GetStream();
		stream.BeginRead(recvBuf, 0, 1, ReceiveCB, null);
	}

	private void ReceiveCB(IAsyncResult result)
	{
		int n = stream.EndRead(result);
		if(n <= 0)
			return;

		nBytesRead = n;
		Debug.Log("Received: " + n);

		for(int i = 0; i < n; i++)
			Receive(recvBuf[i]);
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
