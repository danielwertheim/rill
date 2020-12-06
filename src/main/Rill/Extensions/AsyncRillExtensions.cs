using System.Threading.Tasks;

namespace Rill.Extensions
{
    public static class AsyncRillExtensions
    {
        public static ValueTask<Event<T>> EmitAsync<T>(this IAsyncRill<T> rill, Event<T> ev)
            => rill.EmitAsync(ev.Content, ev.Id, ev.Sequence);
    }
}
