﻿using CommonObjects;
using Messaging.Base;
using Messaging.Base.System_Management.SmartProxy;
using System;

namespace CreditBureauFailOver
{
    public class FailOverControlReceiver<TMessage> : MessageConsumer<TMessage>
    {
        FailOverRouter<TMessage> _failOverRouter;
        private Func<TMessage, FailOverRouteEnum> _getRouteFunc;

        public FailOverControlReceiver(
            IMessageReceiver<TMessage> inputQueue,
            FailOverRouter<TMessage> failOverRouter,
            Func<TMessage, FailOverRouteEnum> getRouteFunc
            )
            : base(inputQueue)
        {
            _failOverRouter = failOverRouter;
            _getRouteFunc = getRouteFunc;
        }

        public override void ProcessMessage(TMessage message)
        {
            FailOverRouteEnum route = _getRouteFunc(message);

            if (_failOverRouter == null)
                return;

            if (route != FailOverRouteEnum.Standby)
            {
                SetRoute(route);

                if (!_failOverRouter.ProcessStarted)
                    _failOverRouter.StartProcessing();
            }
            else
                Console.WriteLine("Routing credit request on-standby");
        }

        void SetRoute(FailOverRouteEnum route)
        {
            _failOverRouter.SwitchDestination(route);
            Console.WriteLine("Route Set: {0}", route.ToString());
        }
    }
}
