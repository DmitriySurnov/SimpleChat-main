using System.Net;

namespace ChatServer
{
    internal class ChatMessage
    {
        private readonly string _name;
        private readonly string _message;
        public ChatMessage(EndPoint name, string message)
        {
            _name = GetIpV4Address(name);
            _message = message;
        }

        public ChatMessage(string message)
        {
            _name = "";
            _message = message;
        }

        private string GetIpV4Address(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var ip = ipEndPoint.Address.MapToIPv4().ToString();
            var port = ipEndPoint.Port;
            return $"{ip}:{port}";
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_name))
                return _message;
            else
                return $"[{_name}] -- {_message}";
        }
    }
}
