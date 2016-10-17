﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Base.System_Management.SmartProxy
{
    public interface ISmartProxyReplyConsumer<TMessageQueue, TMessage, TJournal> : IReplyMessageConsumer<TMessageQueue, TMessage, TJournal>
    {
        void AnalyzeMessage(IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> references, TMessage replyMessage);
    }

    public abstract class SmartProxyReplyConsumer<TMessageQueue, TMessage, TJournal> : MessageConsumer<TMessageQueue, TMessage, TJournal>, ISmartProxyReplyConsumer<TMessageQueue, TMessage, TJournal>
    {
        public SmartProxyReplyConsumer(
            IMessageReceiver<TMessageQueue, TMessage> replyReceiver
            ) : base(replyReceiver)
        {
        }

        public override void ProcessMessage(TMessage message)       //Received reply from service
        {
            //Retrieve message reference
            MessageReferenceData<TMessageQueue, TMessage, TJournal> matchedReferenceData = GetJournalReference(this.ReferenceData, message);

            if (matchedReferenceData != null)
            {
                AnalyzeMessage(this.ReferenceData, message);
                matchedReferenceData.ReplyAddress.Send(message);
                ReferenceData.Remove(matchedReferenceData);
            }
            else
            {
                Console.WriteLine(this.GetType().Name + "Unrecognized Reply Message");
            }
        }

        public abstract void AnalyzeMessage(IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> references, TMessage replyMessage);
        public abstract MessageReferenceData<TMessageQueue, TMessage, TJournal> GetJournalReference(IList<MessageReferenceData<TMessageQueue, TMessage, TJournal>> references, TMessage message);
    }
}
