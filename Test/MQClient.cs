﻿using System;
using System.Messaging;
using CommonObjects;
using MsmqGateway.Core;
using Messaging.Orchestration.Shared.Services;
using MsmqGateway;

namespace Test
{
    class MQClient
    {
        public static IClientService GetClientService(ClientInstance<Message> instance, String[] args)
        {
            int numMessages = 0;
            string requestQueue = string.Empty;
            string replyQueue = string.Empty;
            string mode = string.Empty;
            IMessageFormatter formatter = new XmlMessageFormatter(new Type[] {typeof(LoanQuoteReply)});

            return MQOrchestration.GetInstance().CreateClient(
                args[0],
                "MSMQ",
                ToPath(args[1]),
                ToPath(args[2])
                )
                .Register(registration =>
                {
                    //Server parameter requests
                    registration.RegisterRequiredServerParameters("mode", (value) => mode = (string) value);
                    registration.RegisterRequiredServerParameters("requestQueue",
                        (value) => requestQueue = (string) value);
                    registration.RegisterRequiredServerParameters("replyQueue", (value) => replyQueue = (string) value);
                    registration.RegisterRequiredServerParameters("numMessages",
                        (value) => numMessages = Convert.ToInt32(value));
                },
                    errorMessage =>
                    {
                        //Invalid registration
                    },
                    () =>
                    {
                        //Client parameter setup completed
                        instance.SetupTestLoanBroker(
                            new MessageSenderGateway(ToPath(requestQueue)),
                            new MessageReceiverGateway(ToPath(replyQueue), formatter),
                            numMessages,
                            (message =>
                            {
                                return message.Id;
                            }),
                            ((appSpecificId, request) =>
                            {
                                return new Message(request)
                                {
                                    AppSpecific = appSpecificId
                                };
                            }),
                            (requestObject =>
                            {
                                Message message = (Message) requestObject;

                                return new Tuple<bool, LoanQuoteRequest>(
                                    message.Body is LoanQuoteRequest,
                                    (LoanQuoteRequest) message.Body
                                    );
                            }),
                            (message =>
                            {
                                message.Formatter = formatter;

                                return new Tuple<string, bool, LoanQuoteReply>
                                    (
                                    message.CorrelationId,
                                    message.Body is LoanQuoteReply,
                                    (LoanQuoteReply) message.Body
                                    );
                            })
                            );

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
                        instance.StartProcessing();
                    },
                    () =>
                    {
                        //Stop
                        instance.StopProcessing();
                    });
        }

        private static String ToPath(String arg)
        {
            return ".\\private$\\" + arg;
        }
    }
}