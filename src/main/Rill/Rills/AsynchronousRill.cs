using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rill.Rills
{
    internal sealed class AsynchronousRill<T> : AsyncRillBase<T>
    {
        public AsynchronousRill(RillReference reference) : base(reference)
        {
        }

        protected override async ValueTask<Event<T>> OnEmitAsync(Event<T> ev)
        {
            var exceptions = new List<Exception>();

            foreach (var consumer in Consumers)
            {
                try
                {
                    await consumer.OnNewAsync(ev).ConfigureAwait(false);
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
                        await consumer.OnAllSucceededAsync(ev.Id).ConfigureAwait(false);
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
                //TODO: Log new AggregateException("Exception(s) while notifying consumers of new event.", exceptions.ToArray());
                exceptions.Clear();

                foreach (var consumer in Consumers)
                {
                    try
                    {
                        await consumer.OnAnyFailedAsync(ev.Id).ConfigureAwait(false);
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
