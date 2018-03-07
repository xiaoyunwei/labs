using Senparc.Weixin.MP.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.Context;
using System.IO;
using Senparc.Weixin.MP.Entities.Request;

namespace QRCodeSSO.IdentityWeb.Services
{
    public class WeixinMessageHandler : MessageHandler<MessageContext<IRequestMessageBase, IResponseMessageBase>>
    {
        public WeixinMessageHandler(Stream inputStream, PostModel postModel)
            : base(inputStream, postModel)
        {
            
        }

        // 假如对应类型（如语音）的微信消息没有被代码处理，那么默认会返回这里的结果
        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = $"收到来自{requestMessage.FromUserName}的消息。";
            return responseMessage;
        }

        // 处理文字消息
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = $"收到来自{requestMessage.FromUserName}的消息。\r\n消息内容为：{requestMessage.Content}";      //这里的requestMessage.FromUserName也可以直接写成base.WeixinOpenId
            return responseMessage;
        }

        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            return base.OnEvent_SubscribeRequest(requestMessage);
        }

        public override IResponseMessageBase OnEvent_ScanRequest(RequestMessageEvent_Scan requestMessage)
        {
            return base.OnEvent_ScanRequest(requestMessage);
        }
    }
}
