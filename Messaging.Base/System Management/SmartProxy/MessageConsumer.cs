﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Base.System_Management.SmartProxy
{
    public interface IMessageConsumer<TMessageQueue, TMessage, TJournal>
    {
        IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> ReferenceData { get; set; }

        void Process();
        void ProcessMessage(TMessage message);
    }

    public interface IRequestMessageConsumer<TMessageQueue, TMessage, TJournal> : IMessageConsumer<TMessageQueue, TMessage, TJournal>
    {
        MessageReferenceData<TMessageQueue, TMessage, TJournal> ConstructJournalReference(TMessage message);
    }

    public interface IReplyMessageConsumer<TMessageQueue, TMessage, TJournal> : IMessageConsumer<TMessageQueue, TMessage, TJournal>
    {
        MessageReferenceData<TMessageQueue, TMessage, TJournal> GetJournalReference(IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> references, TMessage message);
    }

    public class MessageReferenceData<TMessageQueue, TMessage, TJournal>
    {
        public TJournal Journal { get; set; }       //Correlation and Id
        public IMessageSender<TMessageQueue, TMessage> ReplyAddress { get; set; }
    }

    public abstract class MessageConsumer<TMessageQueue, TMessage, TJournal> : IMessageConsumer<TMessageQueue, TMessage, TJournal>
    {
        private IMessageCore<TMessageQueue> _messageQueue;

        IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> _references;

        public IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> ReferenceData
        {
            get { return _references; }
            set { _references = value; }
        }

        public MessageConsumer(IMessageSender<TMessageQueue, TMessage> sender)
        {
            _messageQueue = sender;
        }

        public MessageConsumer(IMessageReceiver<TMessageQueue, TMessage> receiver)
        {
            receiver.ReceiveMessageProcessor += new MessageDelegate<TMessage>(ProcessMessage);

            _messageQueue = receiver;
        }

        public void Process()
        {
            if(_messageQueue.GetType() == typeof(IMessageReceiver<TMessageQueue, TMessage>))
                ((IMessageReceiver<TMessageQueue, TMessage>)_messageQueue).StartReceivingMessages();
        }

        public abstract void ProcessMessage(TMessage message);
    }
}
