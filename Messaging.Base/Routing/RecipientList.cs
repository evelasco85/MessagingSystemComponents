﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Messaging.Base.Routing
{    
    public class RecipientList<TBaseEntity, TMessage> : IRecipientList<TBaseEntity, TMessage>
    {
        IList<Tuple<TBaseEntity, IMessageSender<TMessage>>> _recipients = new List<Tuple<TBaseEntity, IMessageSender<TMessage>>>();
        Func<TBaseEntity, IMessageSender<TMessage>> _messageSenderLocator;

        public RecipientList(Func<TBaseEntity, IMessageSender<TMessage>> messageSenderLocator)
        {
            if(messageSenderLocator == null)
                throw new ArgumentNullException("'messageSenderLocator' parameter is required");

            _messageSenderLocator = messageSenderLocator;
        }

        public RecipientList(Func<TBaseEntity, IMessageSender<TMessage>> messageSenderLocator, params TBaseEntity[] recipients)
            : this(messageSenderLocator)
        {
            AddRecipient(recipients);
        }

        public void AddRecipient(params TBaseEntity[] recipients)
        {
            for(int index = 0; index < recipients.Count(); index++)
            {
                AddRecipient(recipients[index]);
            }
        }

        public void AddRecipient(TBaseEntity baseEntity)
        {
            IMessageSender<TMessage> messageSender = _messageSenderLocator(baseEntity);
            Tuple<TBaseEntity, IMessageSender<TMessage>> recipient = new Tuple<TBaseEntity, IMessageSender<TMessage>>(baseEntity, messageSender);

            _recipients.Add(recipient);
        }

        public IList<IMessageSender<TMessage>> GetRecipients(Func<TBaseEntity, bool> recipientCondition)
        {
            if (recipientCondition == null)
                return GetRecipients();

            return _recipients
                .Where(recipient => recipientCondition((recipient.Item1)))
                .Select(recipient => recipient.Item2)
                .ToList();
        }

        public IList<IMessageSender<TMessage>> GetRecipients()
        {
            return GetRecipients((entity) => true);
        }
    }
}
