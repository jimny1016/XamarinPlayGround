using System.Net.Sockets;
using System.Net;
using System;
using Android.Graphics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace FileCatch
{
    public class ConnectSocket
    {
        Socket _clientSocket;
        Socket _serverSocket;
        Func<string, bool> _switchToControlMode;
        Func<bool> _initSerialPort;
        Action _switchBackGround;
        public ConnectSocket(Func<string, bool> switchToControlMode, Func<bool> initSerialPort, Action switchBackGround)
        {
            _switchToControlMode = switchToControlMode;
            _initSerialPort = initSerialPort;
            _switchBackGround = switchBackGround;

            new Thread(new ThreadStart(Socket)) { IsBackground = true }.Start();
        }

        void Socket()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            _serverSocket.Listen(1);
            while (true)
            {
                _clientSocket = _serverSocket.Accept();
                try
                {
                    while (true)
                    {
                        // 接收資料長度
                        int dataSize = ReceiveInt32(_clientSocket);

                        // 回覆確認接收資料長度
                        SendInt32(_clientSocket, "current");

                        // 接收資料
                        byte[] data = ReceiveData(_clientSocket, dataSize);

                        // 處理接收到的資料
                        var command = Encoding.UTF8.GetString(data).Trim();
                        switch (command)
                        {
                            case "SwitchToAndroidControllMode":
                                _switchToControlMode("1");
                                _initSerialPort();
                                break;
                            case "SwitchToSTM32ControllMode":
                                _switchToControlMode("0");
                                break;
                            case "SwitchBackGround":
                                _switchBackGround?.Invoke();
                                break;
                            default:
                                Bitmap decodedByte = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                                break;
                        }

                        // 回覆確認處理完畢
                        SendInt32(_clientSocket, "current");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (_clientSocket.Connected)
                    {
                        _clientSocket?.Shutdown(SocketShutdown.Both);
                        _clientSocket.Disconnect(true);
                        _clientSocket.Close();
                    }
                }
            }
        }

        private int ReceiveInt32(Socket socket)
        {
            byte[] buffer = new byte[4];
            int rec = socket.Receive(buffer);
            return Convert.ToInt32(Encoding.UTF8.GetString(buffer, 0, rec));
        }

        private void SendInt32(Socket socket, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            socket.Send(buffer);
        }

        private byte[] ReceiveData(Socket socket, int dataSize)
        {
            MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = new byte[1024];
            int totalReceived = 0;

            while (totalReceived < dataSize)
            {
                int received = socket.Receive(buffer, Math.Min(buffer.Length, dataSize - totalReceived), SocketFlags.None);
                if (received == 0)
                {
                    // 資料接收完畢或斷線
                    break;
                }
                memoryStream.Write(buffer, 0, received);
                totalReceived += received;
            }

            return memoryStream.ToArray();
        }
    }
}