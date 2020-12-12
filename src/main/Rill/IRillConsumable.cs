using System;

namespace Rill
{
    public interface IRillConsumable<out T> : IDisposable
    {
        /// <summary>
        /// Subscribes a new consumer.
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        IDisposable Subscribe(IRillConsumer<T> consumer);
    }
}
