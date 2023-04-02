namespace ChatServer
{
    internal class ChatMessage
    {
        private readonly string _name;
        private readonly string _message;

        public ChatMessage(string message)
        {
            _name = "";
            _message = message;
        }

        public ChatMessage(string name, string message)
        {
            _name= name;
            _message= message;
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
