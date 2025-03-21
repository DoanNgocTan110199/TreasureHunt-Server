using System.Collections.Generic;
using System.Linq;

namespace Assignment.Models
{
    public class ResponseModel
    {
        public ResponseModel()
        {
        }

        public bool Success { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
        public int Count { get; set; } = 0;
        public void GetResult(List<MessageItem> Messages, string stCode)
        {
            Message = Messages.Where(x => x.id == stCode).ToList().FirstOrDefault().value;
            Code = stCode;
        }

        public void GetResult(List<MessageItem> Messages, string stCode, string strMessage)
        {
            Message = string.Format(Messages.Where(x => x.id == stCode).ToList().FirstOrDefault().value, strMessage);
            Code = stCode;
        }
    }

    public class MessageItem
    {
        public string id { get; set; }
        public string value { get; set; }
    }
    
}
