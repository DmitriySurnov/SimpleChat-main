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
        private static bool _isClientAlive = false;
        private static bool _StopWaitingData = false;
        static void Main(string[] args)
        {
            var socket = ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, 10111));

            var Nickname = GetClientName();
            SendMessageToServer(socket, Nickname);
            ExitTheChatByPressing(socket);
            do
            {
                WaitingData(socket);
                var message = GetClientMessage();

                if (message != "")
                    SendMessageToServer(socket, message);

            } while (!_isClientAlive);
        }


        private static void WaitingData(Socket socket)
        {
            _StopWaitingData = false;
            Enter();
            Console.WriteLine("Waiting for messages");
            bool OutputНeaders = true;
            while (!_StopWaitingData)
            {
                while (socket.Available < 1 && !_StopWaitingData)
                {
                    Thread.Sleep(100);
                }
                if (!_StopWaitingData)
                {
                    if (OutputНeaders)
                    {
                        OutputНeaders = false;
                        Console.WriteLine("---------------Chat content--------------------");
                    }
                    var chatContent = ReceiveChatContent(socket);
                    Console.WriteLine(chatContent);
                }
            }
            if (!OutputНeaders)
            {
                Console.WriteLine("------------End of chat content----------------");
                Console.WriteLine();
            }
        }

        // при нажатие Enter прекрашает ожидать данные из сети
        private static async void Enter()
        {
            await Task.Run(() => WaitingKeyPress(ConsoleKey.Enter));
            _StopWaitingData = true;
        }
        // Ожидает нажатие определенной клавиши. Клавиша которую нужно
        // ожидать нажатие передается как входной параметр
        private static void WaitingKeyPress(ConsoleKey key)
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
            } while (keyInfo.Key != key);
        }

        // при нажатие клавише Esc выходит из приложения.
        private static async void ExitTheChatByPressing(Socket socket)
        {
            await Task.Run(()  => WaitingKeyPress(ConsoleKey.Escape));

            _isClientAlive = true;
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
            Console.WriteLine("");
            Console.Write("Your message:");
            var message = Console.ReadLine();
            return message;
        }

        private static string GetClientName()
        {
            Console.Write("Your name:");
            var message = Console.ReadLine();
            return message;
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
