﻿using System;
using System.Messaging;
using Bank;
using CommonObjects;
using LoanBroker.Bank;
using MsmqGateway.Core;
using Messaging.Base;
using Messaging.Orchestration.Shared.Services;
using MsmqGateway;

namespace LoanBroker
{
    class MQClient
    {
        public static IClientService GetClientService(ClientInstance<MessageQueue, Message> instance, String[] args)
        {
            String requestQueueName = string.Empty;
            String creditRequestQueueName = string.Empty;
            String creditReplyQueueName = string.Empty;
            String bankReplyQueueName = string.Empty;

            string proxyRequestQueue = string.Empty;
            string proxyReplyQueue = string.Empty;
            string controlBusQueue = string.Empty;

            return MQOrchestration.GetInstance().CreateClient(
                args[0],
                "MSMQ",
                ToPath(args[1]),
                ToPath(args[2])
                )
                .Register(registration =>
                {
                    //Server parameter requests
                    registration.RegisterRequiredServerParameters("requestQueueName",
                        (value) => requestQueueName = (string) value);
                    registration.RegisterRequiredServerParameters("creditRequestQueueName",
                        (value) => creditRequestQueueName = (string) value);
                    registration.RegisterRequiredServerParameters("creditReplyQueueName",
                        (value) => creditReplyQueueName = (string) value);
                    registration.RegisterRequiredServerParameters("bankReplyQueueName",
                        (value) => bankReplyQueueName = (string) value);
                    registration.RegisterRequiredServerParameters("controlBusQueue",
                        (value) => controlBusQueue = (string) value);
                    registration.RegisterRequiredServerParameters("proxyRequestQueue",
                        (value) => proxyRequestQueue = (string) value);
                    registration.RegisterRequiredServerParameters("proxyReplyQueue",
                        (value) => proxyReplyQueue = (string) value);

                },
                    errorMessage =>
                    {
                        //Invalid registration
                    },
                    () =>
                    {
                        //Client parameter setup completed
                        /*Proxy Setup*/
                        MessageReceiverGateway<LoanQuoteReply> proxyReplyReceiver = new MessageReceiverGateway<LoanQuoteReply>(ToPath(proxyReplyQueue));
                        IMessageSender<Message> proxyRequestSender = new MessageSenderGateway(ToPath(proxyRequestQueue));
                        MessageReceiverGateway<LoanQuoteRequest> loanRequestReceiver = new MessageReceiverGateway<LoanQuoteRequest>(ToPath(requestQueueName));
                        IMessageSender<Message> controlBus = new MessageSenderGateway(ToPath(controlBusQueue));

                        instance.SetupLoanBrokerProxy(
                            controlBus,
                            new LoanBrokerProxyRequestConsumer(
                                loanRequestReceiver,
                                proxyRequestSender,
                                proxyReplyReceiver.AsReturnAddress(),
                                LoanBrokerProxy<MessageQueue, Message>.SQueueStats
                                ),
                            new LoanBrokerProxyReplyConsumer(
                                proxyReplyReceiver,
                                LoanBrokerProxy<MessageQueue, Message>.SQueueStats,
                                LoanBrokerProxy<MessageQueue, Message>.S_PerformanceStats,
                                controlBus
                                ));
                        /*************/

                        /*Bank Gateway Setup*/
                        MessageReceiverGateway<BankQuoteReply> bankQuoteReplyReceiver = new MessageReceiverGateway<BankQuoteReply>(ToPath(bankReplyQueueName));

                        instance.SetupBankGateway(
                            bankQuoteReplyReceiver,
                            new ConnectionsManager<Message>(),
                            ((aggregationCorrelationID, request) =>
                            {
                                return new Message(request)
                                {
                                    AppSpecific = aggregationCorrelationID
                                };
                            }),
                            (message =>
                            {
                                message.Formatter = new XmlMessageFormatter(new Type[] { typeof(BankQuoteReply) });

                                return new Tuple<bool, BankQuoteReply>(
                                    message.Body is BankQuoteReply,
                                    (BankQuoteReply)message.Body
                                    );
                            }));
                        /********************/

                        /*Credit Bureau Setup*/
                        MessageReceiverGateway<CreditBureauReply> creditBureauReplyReceiver = new MessageReceiverGateway<CreditBureauReply>(ToPath(creditReplyQueueName));

                        instance.SetupCreditBureauInterface(
                            new MessageSenderGateway(ToPath(creditRequestQueueName)),
                            creditBureauReplyReceiver,
                            (message =>
                            {
                                message.Formatter = new XmlMessageFormatter(new Type[] { typeof(CreditBureauReply) });

                                return new Tuple<bool, CreditBureauReply>(
                                    message.Body is CreditBureauReply,
                                    (CreditBureauReply)message.Body
                                    );
                            }));
                        /*********************/

                        instance.SetupProcessManager();

                        IMessageReceiver<MessageQueue, Message> proxyRequestReceiver = new MessageReceiverGateway<LoanQuoteRequest>(ToPath(proxyRequestQueue));
                        MQRequestReplyService_Asynchronous<LoanQuoteRequest> requestReplyService = new MQRequestReplyService_Asynchronous
                            <LoanQuoteRequest>(
                            proxyRequestReceiver,
                            new AsyncProcessMessageDelegate(instance.ProcessManager.ProcessRequestMessage)
                            );

                        instance.SetupQueueService(requestReplyService);
                        instance.HookMessageIdExtractor(proxyRequestReceiver.GetMessageId);
                        instance.StartProcessingManagerListening();

                        Console.WriteLine("Configurations ok!");
                    },
                    () =>
                    {
                        //Stand-by
                        Console.WriteLine("Stand-by Application!");
                    },
                    () =>
                    {
                        //Start
                        instance.Start();
                        Console.WriteLine("Starting Application!");
                    },
                    () =>
                    {
                        //Stop
                        instance.Stop();
                        Console.WriteLine("Stopping Application!");
                    });
        }

        static String ToPath(String arg)
        {
            return ".\\private$\\" + arg;
        }
    }
}
