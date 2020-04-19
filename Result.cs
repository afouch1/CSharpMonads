using System;
using System.Collections.ObjectModel;
using System.Security.Principal;

namespace Monads
{
	public class Result<TOk, TError>
	{
		internal Result() { }

		public static implicit operator Result<TOk, TError>(TOk obj) => Result.Ok<TOk, TError>(obj);

		public static implicit operator Result<TOk, TError>(TError obj) => Result.Error<TOk, TError>(obj);

		public Result<TOutput, TError> Map<TOutput>(Func<TOk, TOutput> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}
		
		public Result<TOutput, TError> Bind<TOutput>(Func<TOk, Result<TOutput, TError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}

		public Result<TOk, TOutError> BindError<TOutError>(Func<TError, Result<TOk, TOutError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => Result.Ok<TOk, TOutError>(ok),
				Error<TOk, TError> err => pred(err)
			};
		}

		public TOk Expect<TException>(string errorMessage) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), errorMessage)
			};
		}
	}

	public sealed class Ok<TOk, TError> : Result<TOk, TError>
	{
		internal Ok(TOk value)
		{
			Value = value;
		}
		
		internal TOk Value { get; }

		public override string ToString() => Value.ToString();

		public static implicit operator TOk(Ok<TOk, TError> ok) => ok.Value;
	}

	public sealed class Error<TOk, TError> : Result<TOk, TError>
	{
		internal Error(TError errorValue)
		{
			ErrorValue = errorValue;
		}
		
		internal TError ErrorValue { get; }

		public override string ToString() => ErrorValue.ToString();

		public static implicit operator TError(Error<TOk, TError> err) => err.ErrorValue;
	}

	public static class Result
	{
		public static Result<TOk, TError> Ok<TOk, TError>(TOk obj) => new Ok<TOk, TError>(obj);
		
		public static Result<TOk, TError> Error<TOk, TError>(TError err) => new Error<TOk, TError>(err);

        public static T Expect<T, TException>(this Result<T, string> res) where TException : Exception
        {
            if (res is Error<T, string> err)
                throw (Exception) Activator.CreateInstance(typeof(TException), err);
            else return (Ok<T, string>) res;
        }
	}
}