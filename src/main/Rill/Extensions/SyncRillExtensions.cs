namespace Rill.Extensions
{
    public static class RillExtensions
    {
        public static Event<T> Emit<T>(this IRill<T> rill, Event<T> ev)
            => rill.Emit(ev.Content, ev.Id, ev.Sequence);
    }
}
