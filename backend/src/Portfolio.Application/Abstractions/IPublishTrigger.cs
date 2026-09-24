namespace Portfolio.Application.Abstractions;

/// <summary>Dispara o rebuild estático do site (GitHub repository_dispatch).</summary>
public interface IPublishTrigger
{
    Task TriggerAsync(CancellationToken ct = default);
}
