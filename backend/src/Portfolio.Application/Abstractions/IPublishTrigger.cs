namespace Portfolio.Application.Abstractions;

/// <summary>
/// Publica o conteúdo: grava o snapshot no S3. O evento do S3 aciona a Lambda
/// PublishDispatcher, que dispara o rebuild estático no GitHub Actions.
/// </summary>
public interface IPublishTrigger
{
    Task PublishAsync(byte[] snapshotJson, CancellationToken ct = default);
}
