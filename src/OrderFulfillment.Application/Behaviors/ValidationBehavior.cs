using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Application.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators):
        IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            if (!validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var resaults = await Task.WhenAll(
                validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = resaults
                .SelectMany(result => result.Errors)
                .ToList();

            if(failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
            return await next(cancellationToken);
        }
    }
    
}
