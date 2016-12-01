﻿using Messaging.Base.Constructions;
using Messaging.Base.System_Management.SmartProxy;

namespace Messaging.Base.System_Management
{
    public abstract class TestMessage<TMessage> : MessageConsumer<TMessage>, ITestMessage<TMessage>
    {
        IMessageSender<TMessage> _controlBusQueue;
        IReturnAddress<TMessage> _monitorQueueReturnAddress;
        IMessageSender<TMessage> _serviceQueue;

        public TestMessage(
            IMessageSender<TMessage> controlBusQueue,
            IMessageSender<TMessage> serviceQueue,
            IMessageReceiver<TMessage> receiver)
            : base(receiver)
        {
            _controlBusQueue = controlBusQueue;
            _monitorQueueReturnAddress = receiver.AsReturnAddress();
            _serviceQueue = serviceQueue;
        }

        public void SendControlBusStatus(TMessage statusMessage)
        {
            _controlBusQueue.Send(statusMessage);
        }

        public void SendControlBusStatus<TEntity>(TEntity statusMessage)
        {
            _controlBusQueue.Send(statusMessage);
        }

        public void SendTestMessage(TMessage message)
        {
            _monitorQueueReturnAddress.SetMessageReturnAddress(ref message);
            _serviceQueue.Send(message);
        }

        public override void ProcessMessage(TMessage message)
        {
            ReceiveTestMessageResponse(message);
        }

        public abstract void ReceiveTestMessageResponse(TMessage message);

        public virtual bool StartProcessing()
        {
            return base.StartProcessing();
        }

        public virtual void StopProcessing()
        {
            base.StopProcessing();
        }
    }
}
