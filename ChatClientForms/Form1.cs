﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace ChatClientForms
{
    public partial class Form1 : Form
    {
        private Socket _socket;
        private bool _stop = false;
        private delegate void AddTextToLogHandle(string text);
        private Thread _currentThread;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SendMessageToServer("");

            DisconnectClientFromServer();

            DisposeClientSocket();
        }

        private void SendMessageToServer(string message)
        {
            SocketUtility.SendString(_socket, message,
                () => { Console.WriteLine($"Send string to server data check client side exception"); });
        }

        private void DisconnectClientFromServer()
        {
            _socket.Disconnect(false);
        }

        private void DisposeClientSocket()
        {
            _stop = true;
            _socket.Close();
            _socket.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, 10111));
            ShowChatContent(ReceiveChatContent());
            SendMessageToServer("forma");
            _currentThread = new Thread(WaitingData);
            _currentThread.Start();
            //ThreadStart(WaitingData).Start(ShowChatContent);
        }

        private void ConnectClientToServer(IPEndPoint serverEndPoint)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.IP);

            _socket.Connect(serverEndPoint);

            label1.Text = $"Client connected Local {_socket.LocalEndPoint} Remote {_socket.RemoteEndPoint}";
        }

        private string ReceiveChatContent()
        {
            string chatContent = SocketUtility.ReceiveString(_socket,
                () => { Console.WriteLine($"Receive string size check from server client side exception"); },
                () => { Console.WriteLine($"Receive string data check from server client side exception"); });
            return chatContent;
        }

        private void ShowChatContent(string chatContent)
        {
            //if (textBox1.Text == "")
            //    textBox1.Text = chatContent;
            //else
            //    textBox1.AppendText(chatContent);
            //textBox1.AppendText(Environment.NewLine);
            if (textBox1.InvokeRequired)
            {
                //rtxtLog.Invoke((AddTextToLogHandle)AddTextToLog, text);
                textBox1.Invoke((AddTextToLogHandle)ShowChatContent, chatContent);
            }
            else
            {
                //textBox1.AppendText($"{DateTime.Now.ToLongTimeString()} => {chatContent}\n");
                textBox1.AppendText(chatContent + Environment.NewLine);
            }
        }

        private void WaitingData()
        {
            //while (!_stop)
            //{
            string strka = "";
            while (!_stop && _socket.Available < 1)
            {
                Thread.Sleep(100);
            }
            if (!_stop)
            {
                strka = ReceiveChatContent();
            }
            if (!_stop)
            {
                ShowChatContent(strka);
            }
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _stop = false;
            //WaitingData(ShowChatContent);
        }
    }

    //    if (rtxtLog.InvokeRequired)
    //            {
    //                rtxtLog.Invoke((AddTextToLogHandle) AddTextToLog, text);
    //            }
    //            else
    //{
    //    rtxtLog.AppendText($"{DateTime.Now.ToLongTimeString()} => {text}\n");
    //}

    //  https://github.com/AndreyChulov/BusesInTown/tree/main/BusesInTown/TownWatchUtility
}
