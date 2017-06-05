﻿using dotSpace.BaseClasses;
using dotSpace.Objects.Network.Messages.Requests;
using dotSpace.Objects.Network.Messages.Responses;
using System.Net;

namespace dotSpace.Objects.Network.Protocols
{

    public class ConnProtocol : ProtocolBase
    {
        public ConnProtocol(NodeBase server) : base(server)
        {
        }

        public override void ProcessRequest(ServerSocket socket, BasicRequest request)
        {
            request = this.ValidateRequest<BasicRequest>(request);
            BasicResponse response = this.operationMap.Execute(request);            
            socket.Send(response);
            socket.Close();
        }

        public override T PerformRequest<T>(IPEndPoint endpoint, BasicRequest request)
        {
            ClientSocket socket = new ClientSocket(endpoint);
            socket.Send(request);
            MessageBase message = socket.Receive<T>();
            socket.Close();
            return this.ValidateResponse<T>(message);
        }

    }
}
