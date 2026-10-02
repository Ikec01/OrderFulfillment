using System;
using System.Collections.Generic;
using System.Text;


namespace OrderFulfillment.Application.Common
{
    public sealed class Result
    {
        private Result(bool isSuccess, ApplicationError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public ApplicationError Error { get; }

        public static Result Success() => new Result(true, ApplicationError.None);
        public static Result Failure(ApplicationError error)
        {
            ArgumentNullException.ThrowIfNull(error);
            if(error == ApplicationError.None)
            {
                throw new ArgumentException("Neuspeh mora imati grešku.", nameof(error)); 
            }

            return new Result(false, error);
        }
    }
}
