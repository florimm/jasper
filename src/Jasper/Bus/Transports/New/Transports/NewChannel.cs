﻿using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Transports.New.Transports
{
    public class NewChannel : IChannel
    {
        private readonly SubscriberAddress _address;
        private readonly ISendingAgent _agent;
        public Uri Uri => _address.Uri;
        public Uri ReplyUri { get; }
        public Uri Destination { get; }
        public Uri Alias => _address.Alias;

        public NewChannel(SubscriberAddress address, Uri replyUri, ISendingAgent agent)
        {
            ReplyUri = replyUri;
            _address = address;
            _agent = agent;

            Destination = _address.Uri;
        }

        public NewChannel(ISendingAgent agent, Uri replyUri)
            : this(new SubscriberAddress(agent.Destination), replyUri, agent)
        {

        }


        public string QueueName()
        {
            return _address.Uri.QueueName();
        }

        public bool ShouldSendMessage(Type messageType)
        {
            return _address.ShouldSendMessage(messageType);
        }

        // TODO -- will need another mechanism here to just enqueue outgoing

        public Task Send(Envelope envelope)
        {
            foreach (var modifier in _address.Modifiers)
            {
                modifier.Modify(envelope);
            }

            return _agent.StoreAndForward(envelope);
        }

        public void Dispose()
        {
            _agent?.Dispose();
        }
    }
}