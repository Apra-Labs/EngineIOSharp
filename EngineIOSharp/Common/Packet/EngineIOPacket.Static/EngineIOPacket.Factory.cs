﻿using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace EngineIOSharp.Common.Packet
{
    partial class EngineIOPacket
    {
        internal static EngineIOPacket CreatePingPacket()
        {
            return new EngineIOPacket()
            {
                Type = EngineIOPacketType.PING,
                IsText = true
            };
        }

        internal static EngineIOPacket CreatePongPacket(string Data = null)
        {
            Data = Data ?? string.Empty;

            return new EngineIOPacket()
            {
                Type = EngineIOPacketType.PONG,
                IsText = true,
                Data = Data,
                RawData = Encoding.UTF8.GetBytes(Data)
            };
        }

        internal static EngineIOPacket CreateErrorPacket(Exception Exception)
        {
            string Data = Exception.ToString();

            return new EngineIOPacket()
            {
                IsText = true,
                Data = Data,
                RawData = Encoding.UTF8.GetBytes(Data)
            };
        }

        internal static EngineIOPacket CreateOpenPacket(string SocketID, int PingInterval, int PingTimeout)
        {
            string Data = new JObject()
            {
                ["sid"] = SocketID,
                ["pingInterval"] = PingInterval,
                ["pingTimeout"] = PingTimeout,
                ["upgrades"] = new JArray()
            }.ToString();

            return new EngineIOPacket()
            {
                Type = EngineIOPacketType.OPEN,
                IsText = true,
                Data = Data,
                RawData = Encoding.UTF8.GetBytes(Data)
            };
        }

        internal static EngineIOPacket CreateMessagePacket(string Data)
        {
            return new EngineIOPacket()
            {
                Type = EngineIOPacketType.MESSAGE,
                IsText = true,
                Data = Data,
                RawData = Encoding.UTF8.GetBytes(Data)
            };
        }

        internal static EngineIOPacket CreateMessagePacket(byte[] RawData)
        {
            return new EngineIOPacket()
            {
                Type = EngineIOPacketType.MESSAGE,
                IsBinary = true,
                Data = BitConverter.ToString(RawData),
                RawData = RawData
            };
        }
    }
}
