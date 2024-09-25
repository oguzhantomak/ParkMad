namespace BuildingBlocks.Events;

public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken cancellationToken) where T : class;
    void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : class;
}

public class EventPublisher : IEventPublisher
{
    private readonly List<Func<object, CancellationToken, Task>> _subscribers = new();

    public void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : class
    {
        _subscribers.Add(async (eventData, token) =>
        {
            if (eventData is T typedEvent)
            {
                await handler(typedEvent, token);
            }
        });
    }

    public async Task Publish<T>(T @event, CancellationToken cancellationToken) where T : class
    {
        foreach (var subscriber in _subscribers)
        {
            await subscriber(@event, cancellationToken);
        }
    }
}