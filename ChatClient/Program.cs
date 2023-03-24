using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace ChatClient
{
    internal class Program
    {
        private static bool _stop = false;
        private static bool _stops = false;
        static void Main(string[] args)
        {
            var socket = ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, 10111));

            var chatContent = ReceiveChatContent(socket);
            ShowChatContent(chatContent);
            Exit(socket);
            do
            {
                WaitingData(socket);
                var message = GetClientMessage();

                if (message != "")
                    SendMessageToServer(socket, message);
                /*
                * Потенциально будет нужна в ходе дальнейшей разработки
                * В текущей версии строку ожидания Enter заменяет ожидание в
                * 1 секунду ниже
                */
                //WaitForEnterPressedToCloseApplication();

            } while (!_stop);
        }

        private static void WaitingData(Socket socket)
        {
            _stops = false;
            Enter();
            Console.WriteLine("Waiting for messages");
            bool start = true;
            while (!_stops)
            {
                while (socket.Available < 1 && !_stops)
                {
                    Thread.Sleep(100);
                }
                if (!_stops)
                {
                    if (start)
                    {
                        start = false;
                        Console.WriteLine("---------------Chat content--------------------");
                    }
                    var chatContent = ReceiveChatContent(socket);
                    Console.WriteLine(chatContent);
                }
            }
            if (!start)
            {
                Console.WriteLine("------------End of chat content----------------");
                Console.WriteLine();
            }
        }

        private static async void Enter()
        {
            await Task.Run(() =>
            {
                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey();
                } while (keyInfo.Key != ConsoleKey.Enter);
            });
            _stops = true;
        }

        private static async void Exit(Socket socket)
        {
            await Task.Run(() =>
            {
                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey();
                } while (keyInfo.Key != ConsoleKey.Escape);
            });
            _stop = true;
            Console.WriteLine("");
            SendMessageToServer(socket, "");

            DisconnectClientFromServer(socket);

            Thread.Sleep(TimeSpan.FromSeconds(1));

            DisposeClientSocket(socket);
            Environment.Exit(0);
        }

        private static void DisposeClientSocket(Socket socket)
        {
            socket.Close();
            socket.Dispose();
        }

        private static void DisconnectClientFromServer(Socket socket)
        {
            socket.Disconnect(false);
            Console.WriteLine("Client disconnected from server");
        }

        private static void WaitForEnterPressedToCloseApplication()
        {
            Console.Write("Press [Enter] to close client console application");
            Console.ReadLine();
        }

        private static void SendMessageToServer(Socket socket, string message)
        {
            if (message == "")
                SocketUtility.SendString(socket, message,
                    () => { Console.WriteLine($"Send string to server data check client side exception"); });
            else
            {
                Console.WriteLine("Sending message to server");
                SocketUtility.SendString(socket, message,
                    () => { Console.WriteLine($"Send string to server data check client side exception"); });
                Console.WriteLine("Message sent to server");
            }
        }

        private static string GetClientMessage()
        {
            Console.Write("Your message:");
            var message = Console.ReadLine();
            return message;
        }

        private static void ShowChatContent(string chatContent)
        {
            Console.WriteLine("---------------Chat content--------------------");
            Console.WriteLine(chatContent);
            Console.WriteLine("------------End of chat content----------------");
            Console.WriteLine();
        }

        private static string ReceiveChatContent(Socket socket)
        {
            string chatContent = SocketUtility.ReceiveString(socket,
                () => { Console.WriteLine($"Receive string size check from server client side exception"); },
                () => { Console.WriteLine($"Receive string data check from server client side exception"); });
            return chatContent;
        }

        private static Socket ConnectClientToServer(IPEndPoint serverEndPoint)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.IP);

            socket.Connect(serverEndPoint);

            Console.WriteLine($"Client connected Local {socket.LocalEndPoint} Remote {socket.RemoteEndPoint}");

            return socket;
        }
    }
}
