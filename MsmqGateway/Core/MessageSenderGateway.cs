using Messaging.Base;
using System;
using System.Messaging;
using Messaging.Base.Constructions;

namespace MsmqGateway.Core
{
    public class MessageSenderGateway : SenderGateway<MessageQueue, Message>
    {
        private IReturnAddress<Message> _returnAddress;

        public MessageSenderGateway(MessageQueueGateway messageQueueGateway) : base(messageQueueGateway)
        {
            _returnAddress = new MQReturnAddress(messageQueueGateway);
        }

        public MessageSenderGateway(String q)
            : this(new MessageQueueGateway(q))
        {
        }

        public MessageSenderGateway(MessageQueue queue)
            : this(new MessageQueueGateway(queue))
        {
        }

        public override IReturnAddress<Message> AsReturnAddress()
        {
            return _returnAddress;
        }

        public override Message SendRawMessage(Message msg)
        {
            return SendMessage(msg);
        }

        public override Message Send<TEntity>(TEntity entity)
        {
            Message message = GetMessage(entity);

            return SendMessage(message);
        }

        public override Message Send<TEntity>(TEntity entity, Action<AssignApplicationIdDelegate, AssignCorrelationIdDelegate> AssignProperty)
        {
            Message message = GetMessage(entity);

            AssignProperty(
                (applicationId) =>
                {
                    message.AppSpecific = Convert.ToInt32(applicationId);
                },
                (correlationId =>
                {
                    message.CorrelationId = correlationId;
                }));

            return SendMessage(message);
        }

        public override Message Send<TEntity>(TEntity entity, IReturnAddress<Message> returnAddress)
        {
            Message message = GetMessage(entity);

            returnAddress.SetMessageReturnAddress(ref message);

            return SendMessage(message);
        }

        public override Message Send<TEntity>(TEntity entity,
            IReturnAddress<Message> returnAddress,
             Action<AssignApplicationIdDelegate, AssignCorrelationIdDelegate, AssignPriorityDelegate> AssignProperty)
        {
            Message message = GetMessage(entity);

            AssignProperty(
                (applicationId) =>
                {
                    message.AppSpecific = Convert.ToInt32(applicationId);
                },
                (correlationId =>
                {
                    message.CorrelationId = correlationId;
                }),
                (priority =>
                {
                    message.Priority = (MessagePriority) priority;
                }));
            returnAddress.SetMessageReturnAddress(ref message);

            return SendMessage(message);
        }

        Message SendMessage(Message message)
        {
            GetQueue().Send(message);

            return message;
        }

        Message GetMessage<TEntity>(TEntity entity)
        {
            return new Message(entity);
        }

        public override void SetupSender()
	    {
            GetQueue().MessageReadPropertyFilter.ClearAll();
            GetQueue().MessageReadPropertyFilter.AppSpecific = true;
            GetQueue().MessageReadPropertyFilter.Body = true;
            GetQueue().MessageReadPropertyFilter.CorrelationId = true;
            GetQueue().MessageReadPropertyFilter.Id = true;
            GetQueue().MessageReadPropertyFilter.ResponseQueue = true;
            GetQueue().MessageReadPropertyFilter.ArrivedTime = true;
            GetQueue().MessageReadPropertyFilter.SentTime = true;
	    }
	}
}
