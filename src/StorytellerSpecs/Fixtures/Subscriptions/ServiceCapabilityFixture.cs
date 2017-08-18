﻿using System;
using System.Collections.Generic;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions.New;
using Jasper.Conneg;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class FakeHandler<T>
    {
        public void Handle(T message)
        {
            // do nothing
        }
    }


    [Hidden]
    public class ServiceCapabilityFixture : BusFixture
    {
        private JasperRegistry _registry;

        private readonly Dictionary<Type, Type> _handlerTypes = new Dictionary<Type, Type>
        {
            {typeof(Message1), typeof(Message1Handler) },
            {typeof(Message2), typeof(Message2Handler) },
            {typeof(Message3), typeof(Message3Handler) },
            {typeof(Message4), typeof(Message4Handler) },
            {typeof(Message5), typeof(Message5Handler) },
            {typeof(Message6), typeof(Message6Handler) },
            {typeof(ErrorMessage), typeof(ErrorMessageHandler) },

        };


        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _registry.Messaging.Handlers.ConventionalDiscoveryDisabled = true;
        }

        public override void TearDown()
        {
            _registry.Settings.Alter<BusSettings>(_ =>
            {
                _.DisableAllTransports = true;
                _.ThrowOnValidationErrors = false;
            });

            ServiceCapabilities capabilities = null;
            using (var runtime = JasperRuntime.For(_registry))
            {
                capabilities = runtime.Capabilities;
            }

            Context.State.RetrieveOrAdd(() => new List<ServiceCapabilities>())
                .Add(capabilities);



        }

        [ExposeAsTable("The message types handled in the service are")]
        public void HandlesMessages([SelectionList("MessageTypes")] string MessageType)
        {
            var type = messageTypeFor(MessageType);

            var handlerType = _handlerTypes[type];

            _registry.Messaging.Handlers.IncludeType(handlerType);
        }

        [FormatAs("Subscribes to all handled messages at {destination}")]
        public void SubscribeToAllHandledMessages(Uri destination)
        {
            _registry.Subscriptions.ToAllMessages().At(destination);
        }

        [FormatAs("Subscribe to all handled messages that start with 'Message'")]
        public void SubscribeToAllMessagesStartingWithM()
        {
            _registry.Subscriptions.To(t => t.Name.StartsWith("Message"));
        }

        [FormatAs("Service Name is {serviceName}")]
        public void ServiceNameIs(string serviceName)
        {
            _registry.ServiceName = serviceName;
        }

        [FormatAs("Publishes message {messageType}")]
        public void Publishes(
            [SelectionList("MessageTypes")]string messageType)
        {
            PublishesWithExtraContentTypes(messageType, new string[0]);
        }

        [FormatAs("Publishes message {MessageType} with extra content types {contentTypes}")]
        public void PublishesWithExtraContentTypes(
            [SelectionList("MessageTypes")]string MessageType,
            string[] contentTypes)
        {
            var type = messageTypeFor(MessageType);
            _registry.Publishing.Message(type);

            foreach (var contentType in contentTypes)
            {
                var writer = new StubWriter(type, contentType);
                _registry.Services.For<IMediaWriter>().Add(writer);
            }
        }

        [FormatAs("The default subscription receiver is {uri}")]
        public void DefaultSubscriptionReceiverIs([SelectionList("Channels")]string uri)
        {
            _registry.Subscriptions.At(uri);
        }


        [FormatAs("Subscribes to message {messageType}")]
        public void SubscribesTo([SelectionList("MessageTypes")] string messageType)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscriptions.To(type);
        }

        [FormatAs("Subscribes to message {messageType} at {receiver}")]
        public void SubscribesAtLocation(
            [SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] string receiver)
        {
            var type = messageTypeFor(messageType);
            _registry.Subscriptions.To(type).At(receiver);


        }

        [ExposeAsTable("The custom media readers are")]
        public void CustomReadersAre(
            [SelectionList("MessageTypes"), Header("Message Type")] string messageType,
            [Header("Content Types")]string[] contentTypes)
        {
            var type = messageTypeFor(messageType);

            foreach (var contentType in contentTypes)
            {
                var reader = new StubReader(type, contentType);
                _registry.Services.For<IMediaReader>().Add(reader);
            }
        }

        [FormatAs("Additional transport schemes are {schemes}")]
        public void AdditionalTransportsAre(string[] schemes)
        {
            foreach (var scheme in schemes)
            {
                var transport = new StubTransport(scheme);
                _registry.Services.For<ITransport>().Add(transport);
            }
        }

    }
}
