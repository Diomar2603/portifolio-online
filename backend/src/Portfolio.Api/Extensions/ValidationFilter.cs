using FluentValidation;

namespace Portfolio.Api.Extensions;

/// <summary>Aplica o validador FluentValidation do tipo T e devolve 400 (ProblemDetails) se inválido.</summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        var model = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (validator is not null && model is not null)
        {
            var result = await validator.ValidateAsync(model, ctx.HttpContext.RequestAborted);
            if (!result.IsValid) return TypedResults.ValidationProblem(result.ToDictionary());
        }
        return await next(ctx);
    }
}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter<ValidationFilter<T>>();
}
