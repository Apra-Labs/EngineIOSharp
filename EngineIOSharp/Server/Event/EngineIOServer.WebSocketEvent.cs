﻿using EngineIOSharp.Client;
using EngineIOSharp.Client.Event;
using EngineIOSharp.Common;
using EngineIOSharp.Common.Packet;
using EngineIOSharp.Server.Event;
using SimpleThreadMonitor;
using System;
using System.Collections.Generic;
using WebSocketSharp.Net.WebSockets;
using WebSocketSharp.Server;

namespace EngineIOSharp.Server
{
    partial class EngineIOServer
    {
        private readonly List<EngineIOClient> ClientList = new List<EngineIOClient>();
        private readonly object ClientMutex = new object();

        private EngineIOBehavior CreateBehavior()
        {
            return new EngineIOBehavior((WebSocketContext Context) =>
            {
                SimpleMutex.Lock(ClientMutex, () =>
                {
                    string SID = Context.QueryString["sid"] ?? EngineIOSocketID.Generate();
                    EngineIOClient Client = new EngineIOClient(Context, SID);

                    if (!HeartbeatMutex.ContainsKey(Client))
                    {
                        Client.On(EngineIOClientEvent.CLOSE, () =>
                        {
                            SimpleMutex.Lock(ClientMutex, () =>
                            {
                                if (Client != null)
                                {
                                    ClientList.Remove(Client);
                                    StopHeartbeat(Client);
                                }
                            });
                        });

                        Client.On(EngineIOClientEvent.UPGRADE, () =>
                        {
                            SimpleMutex.Lock(ClientMutex, () =>
                            {
                                SIDList.Remove(Client.SID);
                            });
                        });

                        Client.On(EngineIOClientEvent.FLUSH, (Packet) =>
                        {
                            if (Packet.Type == EngineIOPacketType.PONG)
                            {
                                SimpleMutex.Lock(ClientMutex, () =>
                                {
                                    LockHeartbeat(Client, () =>
                                    {
                                        if (!Heartbeat.ContainsKey(Client))
                                        {
                                            Heartbeat.TryAdd(Client, 0);
                                        }

                                        Heartbeat[Client]++;
                                    });
                                });
                            }
                        });

                        if (!SIDList.Contains(SID))
                        {
                            Client.Send(EngineIOPacket.CreateOpenPacket(Client.SID, PingInterval, PingTimeout));
                        }

                        ClientList.Add(Client);
                        StartHeartbeat(Client);

                        CallEventHandler(EngineIOServerEvent.CONNECTION, Client);
                    }
                });
            });
        }

        private class EngineIOBehavior : WebSocketBehavior
        {
            private readonly Action<WebSocketContext> Initializer;

            internal EngineIOBehavior(Action<WebSocketContext> Initializer)
            {
                this.Initializer = Initializer;
            }

            protected override void OnOpen()
            {
                if (ID != null && Sessions[ID]?.Context != null)
                {
                    Initializer(Sessions[ID].Context);
                }
            }
        }
    }
}