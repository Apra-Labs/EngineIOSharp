﻿using EngineIOSharp.Common;
using EngineIOSharp.Common.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineIOSharp.Client
{
    partial class EngineIOClient
    {
        private int AutoReconnectionCount = 0;

        private void OnWebsocketOpen(object sender, EventArgs e)
        {
        }

        private void OnWebsocketClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            CallEventHandler(EngineIOEvent.CLOSE);

            if (AutoReconnect > AutoReconnectionCount)
            {
                AutoReconnectionCount++;
                Connect();
            }
        }

        private void OnWebsocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            CallEventHandler(EngineIOEvent.ERROR, EngineIOPacket.CreateErrorPacket(e.Message));
        }

        private void OnWebsocketMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            EngineIOPacket Packet = null;

            if (e.IsText)
            {
                Packet = EngineIOPacket.Decode(e.Data);
            }
            else if (e.IsBinary)
            {
                Packet = EngineIOPacket.Decode(e.RawData);
            }

            HandleEnginePacket(Packet);
        }
    }
}
