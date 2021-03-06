﻿using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Jimu.Client
{
    class ReadClientMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        public ReadClientMessageChannelHandlerAdapter(ISerializer serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            try
            {
                byte[] data = new byte[buffer.ReadableBytes];
                buffer.GetBytes(buffer.ReaderIndex, data);

                _logger.Debug($"received msg is: {Encoding.UTF8.GetString(data)}");
                var convertedMsg = _serializer.Deserialize<byte[], JimuTransportMsg>(data);
                if (convertedMsg.ContentType == typeof(JimuRemoteCallData).FullName)
                {
                    convertedMsg.Content = _serializer.Deserialize<string, JimuRemoteCallData>(convertedMsg.Content.ToString());
                }
                else if (convertedMsg.ContentType == typeof(JimuRemoteCallData).FullName)
                {
                    convertedMsg.Content = _serializer.Deserialize<string, JimuRemoteCallResultData>(convertedMsg.Content.ToString());
                }
                context.FireChannelRead(convertedMsg);
            }
            finally
            {
                buffer.Release();
            }
            //base.ChannelRead(context, message);
        }
    }
}
