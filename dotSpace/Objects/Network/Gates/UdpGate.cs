﻿using dotSpace.BaseClasses;
using dotSpace.Interfaces;
using dotSpace.Objects.Network.Protocols;
using System;
using System.Net.Sockets;
using System.Threading;

namespace dotSpace.Objects.Network.Gates
{
    public sealed class UdpGate : GateBase
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Fields

        private UdpClient listener;
        private bool listening;

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Constructors

        public UdpGate(IEncoder encoder, GateInfo gateInfo) : base(encoder, gateInfo)
        {
            this.listener = new UdpClient();
            this.listening = false;
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Public Methods

        public override void Start(Action<IConnectionMode> callback)
        {
            if (!this.listening)
            {
                this.listening = true;
                IConnectionMode mode = this.GetMode(this.gateInfo.Mode, new Udp(listener, this.gateInfo.Host, this.gateInfo.Port));
                new Thread(() => { callback(mode); }).Start();
            }
        }

        public override void Stop()
        {
            this.listening = false;
        }

        #endregion
    }
}
