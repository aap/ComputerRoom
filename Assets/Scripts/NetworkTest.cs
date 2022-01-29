using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class NetworkTest : MonoBehaviour
{
	TcpClient socket;
	NetworkStream stream;
	byte[] recvBuf;
	byte[] sendBuf;

	public int l_ir;
	public int l_milt;
	public int l_mirt;
	public int l_pc;
	public int l_ma;
	public int l_run;
	public int l_memstop;
	public int l_pion;
	public int l_pih;
	public int l_pir;
	public int l_pio;

	public int l_mblt;
	public int l_mbrt;
	public int l_arlt;
	public int l_arrt;
	public int l_mqlt;
	public int l_mqrt;

	public int[] l_flags;

	public int key_start;
	public int key_readin;
	public int key_inst_cont;
	public int key_mem_cont;
	public int key_inst_stop;
	public int key_mem_stop;
	public int key_io_reset;
	public int key_exec;
	public int key_dep;
	public int key_dep_nxt;
	public int key_ex;
	public int key_ex_nxt;
	public int key_rd_off;
	public int key_rd_on;
	public int key_pt_rd;
	public int key_pt_wr;

	public int sw_dataLT;
	public int sw_dataRT;
	public int sw_mas;
	public int sw_addr_stop;
	public int sw_repeat;
	public int sw_mem_disable;
	public int sw_power;

	public int dectape_motion;

	// poor man's octal
	const int o77 = 7*8 + 7;
	const int o100 = 1*8*8;
	const int o177 = 1*8*8 + 7*8 + 7;

	void Start()
	{
		recvBuf = new byte[1024];
		sendBuf = new byte[1024];
		l_flags = new int[14];
		socket = new TcpClient {
			ReceiveBufferSize = 1024,
			SendBufferSize = 1024
		};
		socket.BeginConnect("soma", 2000, ConnectCB, null);
	}

	private void ConnectCB(IAsyncResult result)
	{
		socket.EndConnect(result);
		if(!socket.Connected)
			return;

		stream = socket.GetStream();

		StartComm();

/*
		string msg = "Hello from unity\n";
		int len = msg.Length;
		Array.Copy(System.Text.Encoding.UTF8.GetBytes(msg), sendBuf, len);
		stream.BeginWrite(sendBuf, 0, len, SendCB, null);
*/
	}

	private void StartComm()
	{
		sendBuf[0] = (byte)(((sw_dataLT>>12) & o77) | o100);
		sendBuf[1] = (byte)(((sw_dataLT>>6) & o77) | o100);
		sendBuf[2] = (byte)(((sw_dataLT>>0) & o77) | o100);
		sendBuf[3] = (byte)(((sw_dataRT>>12) & o77) | o100);
		sendBuf[4] = (byte)(((sw_dataRT>>6) & o77) | o100);
		sendBuf[5] = (byte)(((sw_dataRT>>0) & o77) | o100);
		sendBuf[6] = (byte)(((sw_mas>>12) & o77) | o100);
		sendBuf[7] = (byte)(((sw_mas>>6) & o77) | o100);
		sendBuf[8] = (byte)(((sw_mas>>0) & o77) | o100);
		sendBuf[9] = (byte)(o100 | (key_readin<<5) | (key_mem_cont<<4) | (key_mem_stop<<3) |
			(key_exec<<2) | (key_dep_nxt<<1) | (key_ex_nxt<<0));
		sendBuf[10] = (byte)(o100 | (key_start<<5) | (key_inst_cont<<4) | (key_inst_stop<<3) |
			(key_io_reset<<2) | (key_dep<<1) | (key_ex<<0));
		sendBuf[11] = (byte)(o100 | (key_rd_off<<3) | (key_rd_on<<2) | (key_pt_rd<<1) | (key_pt_wr<<0));
		sendBuf[12] = (byte)(o100 | (sw_addr_stop<<3) | (sw_repeat<<2) | (sw_mem_disable<<1) | (sw_power<<0));
		sendBuf[13] = (byte)o100;	// speed knob
		sendBuf[14] = 0x13;

		stream.BeginRead(recvBuf, 0, 16, ReceiveCB, null);
		stream.BeginWrite(sendBuf, 0, 15, SendCB, null);
	}

	private void SendCB(IAsyncResult result)
	{
		stream.EndWrite(result);
	}

	private void ReceiveCB(IAsyncResult result)
	{
		int n = stream.EndRead(result);
		if(n <= 0)
			return;

		for(int i = 0; i < n; i++) {
			int t = ((recvBuf[i+1]&o77)<<12) | ((recvBuf[i+2]&o77)<<6) | ((recvBuf[i+3]&o77));
			int tt = ((recvBuf[i+4]&o77)<<12) | ((recvBuf[i+5]&o77)<<6) | ((recvBuf[i+6]&o77));
			switch(recvBuf[i]) {
			case 0x30:
				l_ir = t;
				i += 3;
				break;
			case 0x31:
				l_milt = t;
				i += 3;
				break;
			case 0x32:
				l_mirt = t;
				i += 3;
				break;
			case 0x33:
				l_pc = t;
				i += 3;
				break;
			case 0x34:
				l_ma = t;
				i += 3;
				break;
			case 0x35:
				l_run = (t>>17)&1;
				l_pih = (t>>9) & o177;
				i += 3;
				break;
			case 0x36:
				l_memstop = (t>>17)&1;
				l_pir = (t>>9) & o177;
				i += 3;
				break;
			case 0x37:
				l_pion = (t>>17)&1;
				l_pio = (t>>9) & o177;
				i += 3;
				break;
			case 0x38:
				int m = 0;
				m |= (recvBuf[i+1]&3)<<0;
				m |= (recvBuf[i+2]&3)<<2;
				m |= (recvBuf[i+3]&3)<<4;
				m |= (recvBuf[i+4]&3)<<6;
				dectape_motion = m;
				i += 4;
				break;

			case 0x21:
				l_mblt = t;
				l_mbrt = tt;
				i += 6;
				break;
			case 0x22:
				l_arlt = t;
				l_arrt = tt;
				i += 6;
				break;
			case 0x23:
				l_mqlt = t;
				l_mqrt = tt;
				i += 6;
				break;
			case 0x24:
				for(int j = 0; j < 14; j++)
					l_flags[j] = recvBuf[1+j];
				break;
			}
		}

//		string s = System.Text.Encoding.UTF8.GetString(recvBuf, 0, n);
//		Debug.Log(s);
//		stream.BeginRead(recvBuf, 0, 16, ReceiveDB, null);
		StartComm();
	}

	void Update()
	{
	}
}
