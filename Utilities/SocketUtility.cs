﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Utilities
{
    public static class SocketUtility
    {

        public static string ReceiveString(Socket socket)
        {
            WaitDataFromSocket(socket);
            using (Stream dataStream = new MemoryStream())
            using (BinaryReader dataStreamReader = new BinaryReader(dataStream))
            {
                byte[] dataBuffer = new byte[1024];
                socket.Receive(dataBuffer);
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(dataBuffer, 0, sizeof(long));
                dataStream.Seek(0, SeekOrigin.Begin);
                var razmer = dataStreamReader.ReadInt64();
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.Write(dataBuffer, 8, Convert.ToInt32(razmer) + 1);
                dataStream.Seek(0, SeekOrigin.Begin);
                return dataStreamReader.ReadString();
            }
        }

        public static string ReceiveString(Socket socket, 
            Action onReceiveDataSizeCheckFail, Action onReceiveDataCheckFail)
        {
            using (Stream dataStream = new MemoryStream())
            using (BinaryReader dataStreamReader = new BinaryReader(dataStream))
            {
                var dataSize = ReceiveDataSize(socket, dataStream, dataStreamReader, onReceiveDataSizeCheckFail);
                ReceiveDataToStream(socket, dataSize, dataStream, onReceiveDataCheckFail);
                
                dataStream.Seek(0, SeekOrigin.Begin);
                return dataStreamReader.ReadString();
            }
        }
        
        private static void ReceiveDataToStream(
            Socket socket, long dataSize, 
            Stream dataStream, Action onReceiveDataCheckFail)
        {
            var maxBufferSize = 1024;
            var remainingDataSize = dataSize;

            dataStream.Seek(0, SeekOrigin.Begin);
            
            while (remainingDataSize > maxBufferSize)
            {
                ReceiveBufferToStream(socket, dataStream, maxBufferSize, onReceiveDataCheckFail);

                remainingDataSize -= maxBufferSize;
            }
            
            ReceiveBufferToStream(socket, dataStream, (int)remainingDataSize, onReceiveDataCheckFail);
        }

        private static void ReceiveBufferToStream(
            Socket socket, Stream dataStream, int bufferSize,
            Action onReceiveDataCheckFail)
        {
            WaitDataFromSocket(socket, bufferSize);

            byte[] dataBuffer = new byte[bufferSize];
            var receivedBufferSize = socket.Receive(dataBuffer);

            if (receivedBufferSize != bufferSize)
            {
                onReceiveDataCheckFail();
            }

            dataStream.Write(dataBuffer, 0, bufferSize);
        }

        private static long ReceiveDataSize(Socket socket, Stream dataStream, 
            BinaryReader dataStreamReader, Action onReceiveDataCheckFail)
        {
            WaitDataFromSocket(socket, sizeof(long));

            byte[] dataBuffer = new byte[sizeof(long)];
            var receivedBufferSize = socket.Receive(dataBuffer);

            if (receivedBufferSize != dataBuffer.Length)
            {
                onReceiveDataCheckFail();
            }

            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.Write(dataBuffer, 0, dataBuffer.Length);
            dataStream.Seek(0, SeekOrigin.Begin);
            return dataStreamReader.ReadInt64();
        }

        public static void WaitDataFromSocket(Socket clientSocket)
        {
            WaitDataFromSocket(clientSocket, 1);
        }

        private static void WaitDataFromSocket(Socket clientSocket, int waitForBytesAvailable)
        {
            while (clientSocket.Available < waitForBytesAvailable)
            {
                Thread.Sleep(100);
            }
        }

        public static void SendString(Socket socket, string dataToSend, Action onSendDataCheckFail)
        {
            using (Stream dataStream = new MemoryStream())
            using (BinaryWriter dataStreamWriter = new BinaryWriter(dataStream))
            {
                /*
                 * записываем пустышку вместо размера пакета данных,
                 * на данном этапе мы не знаем размер отправляемых данных
                 */
                dataStreamWriter.Write((long)0);
                
                dataStreamWriter.Write(dataToSend);
                dataStreamWriter.Flush();
            
                byte[] sendDataBuffer = new byte[dataStream.Position];

                /*
                 * Перезаписываем актуальный размер пакета данных,
                 * теперь мы знаем его размер
                 */
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStreamWriter.Write(dataStream.Length - sizeof(long));
                
                dataStream.Seek(0, SeekOrigin.Begin);
                int readBytesFromMemoryStream = dataStream.Read(sendDataBuffer, 0, sendDataBuffer.Length);

                if (readBytesFromMemoryStream != sendDataBuffer.Length)
                {
                    onSendDataCheckFail();
                }

                socket.Send(sendDataBuffer);
            }
        }

    }
}