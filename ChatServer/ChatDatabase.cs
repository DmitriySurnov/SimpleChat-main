using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    internal static class ChatDatabase
    {
        private static List<ChatMessage> _chatLines = new List<ChatMessage>()
        {
            new ChatMessage("Welcome to our best chat ever:"),
            new ChatMessage("----------------------------"),
            new ChatMessage("Enter your joke now:"),
            new ChatMessage("----------------------------"),
        };

        public static void AddMessage(string message, string name)
        {
            _chatLines.Add(new ChatMessage(name, message));
        }

        public static string GetChat()
        {
            return _chatLines
                .Aggregate("", (accumulate, line) => $"{accumulate}\n{line}")
                .TrimStart('\n');
        }
    }
}
