﻿using System;
using CommonObjects;
using Messaging.Base.Construction;
using Messaging.Base.Routing;

namespace LoanBroker.LoanBroker
{
    internal class ProcessManager<TMessage>
    {
        IProcessManager<string, Process<TMessage>, ProcessManager<TMessage>> _manager;
        private IRequestReply_Asynchronous<TMessage> _queueService;
        private Func<TMessage, string> _extractProcessIdFunc;
        private BankGateway<TMessage> _bankInterface;
        private ICreditBureauGateway _creditBureauInterface;

        public ProcessManager(
            BankGateway<TMessage> bankInterface,
            ICreditBureauGateway creditBureauInterface
            )
        {
            _bankInterface = bankInterface;
            _creditBureauInterface = creditBureauInterface;
            _manager = new ProcessManager<string, Process<TMessage>, ProcessManager<TMessage>>(
                this,
                ChildProcessNotification
                );
        }

        public BankGateway<TMessage> BankInterface
        {
            get { return _bankInterface; }
        }

        public ICreditBureauGateway CreditBureauInterface
        {
            get { return _creditBureauInterface; }
        }

        public void ProcessRequestMessage(Object o, TMessage incomingMessage)
        {
            LoanQuoteRequest quoteRequest = (LoanQuoteRequest)o;

            String processID = _extractProcessIdFunc(incomingMessage);
            Process<TMessage> newProcess = new Process<TMessage>(processID, quoteRequest, incomingMessage);

            _manager.AddProcess(newProcess);

            newProcess.StartProcess();
        }

        public void HookProcessIdExtractor(Func<TMessage, string> extractProcessIdFunc)
        {
            _extractProcessIdFunc = extractProcessIdFunc;
        }

        public void HookQueueService(IRequestReply_Asynchronous<TMessage> queueService)
        {
            _queueService = queueService;
        }

        public void SendReply(Object responseObject, TMessage originalRequestMessage)
        {
            if(_queueService != null)
                _queueService.SendReply(responseObject, originalRequestMessage);
        }

        void ChildProcessNotification(IProcess<string, Process<TMessage>, ProcessManager<TMessage>> process)
        {
            _manager.RemoveProcess(process);
            Console.WriteLine("Current outstanding aggregate count: {0}", _bankInterface.GetOutstandingAggregateCount());
        }
    }
}
