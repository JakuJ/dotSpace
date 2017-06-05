﻿using dotSpace.Interfaces;
using System;
using System.Net;
using System.Net.Sockets;

namespace dotSpace.BaseClasses
{
    public abstract class NodeBase
    {
        protected string address;
        protected int port;

        public NodeBase (string address, int port)
        {
            this.address = string.IsNullOrEmpty(address) ? this.GetLocalIPAddress() : address;
            this.port = port;
        }

        public abstract ITuple Get(string target, IPattern pattern);
        public abstract ITuple GetP(string target, IPattern pattern);
        public abstract ITuple Query(string target, IPattern pattern);
        public abstract ITuple QueryP(string target, IPattern pattern);
        public abstract void Put(string target, ITuple t);

        public IPEndPoint CreateEndpoint()
        {
            IPAddress ipAddress = IPAddress.Parse(this.address);
            return new IPEndPoint(ipAddress, this.port);
        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
