using System;

namespace Rill
{
    public interface IAsyncRillConsumable<out T> : IDisposable
    {
        /// <summary>
        /// Subscribes a new consumer.
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        IDisposable Subscribe(IAsyncRillConsumer<T> consumer);
    }
}
