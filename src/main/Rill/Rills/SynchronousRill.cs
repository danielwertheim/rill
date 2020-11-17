using System;
using System.Collections.Generic;
using System.Linq;

namespace Rill.Rills
{
    internal sealed class SynchronousRill<T> : SyncRillBase<T>
    {
        protected override Event<T> OnEmit(Event<T> ev)
        {
            var exceptions = new List<Exception>();

            foreach (var consumer in Consumers)
            {
                try
                {
                    consumer.OnNew(ev);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!exceptions.Any())
            {
                foreach (var consumer in Consumers)
                {
                    try
                    {
                        consumer.OnAllSucceeded(ev.Id);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Any())
                    throw new AggregateException("Exception(s) while notifying consumers of successful event.", exceptions);
            }
            else
            {
                var aex = new AggregateException("Exception(s) while notifying consumers of new event.", exceptions.ToArray());
                exceptions.Clear();

                foreach (var consumer in Consumers)
                {
                    try
                    {
                        consumer.OnAnyFailed(ev.Id, aex);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Any())
                    throw new AggregateException("Exception(s) while notifying consumers of failing event.", exceptions);
            }

            return ev;
        }
    }
}
