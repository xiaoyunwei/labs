using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebChat.Data
{
    public interface IChatService
    {
        string GetReplyText(string message);
    }

    public class ChatService : IChatService
    {
        public string GetReplyText(string message)
        {
            string resp = "";

            if (!string.IsNullOrEmpty(message))
            {
                resp = message.Trim().Replace("吗", "")
                  .Replace("？", "！")
                  .Replace("?", "!");
            }

            return resp;
        }        
    }
}
