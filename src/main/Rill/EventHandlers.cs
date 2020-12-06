using System;
using System.Threading.Tasks;

namespace Rill
{
    /// <summary>
    /// Handler used to handle the dispatch of a new event in an async consumer.
    /// </summary>
    /// <param name="ev"></param>
    /// <typeparam name="T"></typeparam>
    public delegate ValueTask AsyncNewEventHandler<T>(Event<T> ev);
    /// <summary>
    /// Handler used to handle the notification of a successful event dispatch in an async consumer.
    /// </summary>
    /// <param name="eventId"></param>
    public delegate ValueTask AsyncSuccessfulEventHandler(EventId eventId);
    /// <summary>
    /// Handler used to handle the notification of a failed event dispatch in an async consumer.
    /// </summary>
    /// <param name="eventId"></param>
    public delegate ValueTask AsyncFailedEventHandler(EventId eventId);

    /// <summary>
    /// Handler used to handle the dispatch of a new event in a consumer.
    /// </summary>
    /// <param name="ev"></param>
    /// <typeparam name="T"></typeparam>
    public delegate void NewEventHandler<T>(Event<T> ev);
    /// <summary>
    /// /// Handler used to handle the notification of a successful event dispatch in a consumer.
    /// </summary>
    /// <param name="eventId"></param>
    public delegate void SuccessfulEventHandler(EventId eventId);
    /// <summary>
    /// Handler used to handle the notification of a failed event dispatch in a consumer.
    /// </summary>
    /// <param name="eventId"></param>
    public delegate void FailedEventHandler(EventId eventId);
}
