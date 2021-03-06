﻿using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence.Outbox
{
    public class OutboxSender : JasperRegistry
    {
        public OutboxSender(MessageTracker tracker)
        {

            Handlers.DisableConventionalDiscovery().IncludeType<CascadeReceiver>();
            Services.AddSingleton(tracker);
            Publish.Message<TriggerMessage>().To("durable://localhost:2337");
            Transports.DurableListenerAt(2338);

            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString, "outbox_sender");

        }
    }

    public class OutboxReceiver : JasperRegistry
    {
        public OutboxReceiver()
        {

            Handlers.DisableConventionalDiscovery().IncludeType<TriggerMessageReceiver>();

            Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString, "outbox_receiver");


            Transports.DurableListenerAt(2337);
        }
    }


    public class cascading_message_with_outbox : IDisposable
    {
        public cascading_message_with_outbox()
        {
            theSender = JasperRuntime.For(new OutboxSender(theTracker));
            theSender.RebuildMessageStorage();

            theReceiver = JasperRuntime.For<OutboxReceiver>();
            theReceiver.RebuildMessageStorage();
        }

        public void Dispose()
        {
            theReceiver?.Dispose();
            theSender?.Dispose();
        }

        private readonly MessageTracker theTracker = new MessageTracker();
        private readonly JasperRuntime theReceiver;
        private readonly JasperRuntime theSender;

        [Fact]
        public async Task send_end_to_end_and_back_with_cascading_message()
        {
            var trigger = new TriggerMessage {Name = "Ronald"};

            var waiter = theTracker.WaitFor<CascadedMessage>();

            await theSender.Messaging.Send(trigger);

            waiter.Wait(10.Seconds());

            waiter.Result.ShouldNotBeNull();
            waiter.Result.Message.ShouldBeOfType<CascadedMessage>()
                .Name.ShouldBe("Ronald");
        }
    }

    public class TriggerMessage
    {
        public string Name { get; set; }
    }

    public class CascadedMessage
    {
        public string Name { get; set; }
    }

    public class CascadeReceiver
    {
        public void Handle(CascadedMessage message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
    }

    public class TriggerMessageReceiver
    {
        [SqlTransaction]
        public object Handle(TriggerMessage message, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return new RespondToSender(response);
        }
    }

    public static class HandlerConfigurationExtensions
    {
        public static IHandlerConfiguration DisableConventionalDiscovery(this IHandlerConfiguration handlers, bool disabled = true)
        {
            if (disabled)
            {
                handlers.Discovery(x => x.DisableConventionalDiscovery());
            }

            return handlers;
        }

        public static IHandlerConfiguration OnlyType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<T>();
            });

            return handlers;
        }

        public static IHandlerConfiguration IncludeType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x => x.IncludeType<T>());

            return handlers;
        }
    }
}
