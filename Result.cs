using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;

namespace Monads
{
	public class Result<TOk, TError>
	{
		internal Result() { }

		public static implicit operator Result<TOk, TError>(TOk obj) => Result.Ok<TOk, TError>(obj);

		public static implicit operator Result<TOk, TError>(TError obj) => Result.Error<TOk, TError>(obj);
		
		#region Higher Order Functions
		
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

		public Result<TOk, TError> OnSuccess(Action<TOk> onsuccess)
		{
			if (this is Ok<TOk, TError> ok) onsuccess(ok);
			return this;
		}

		public Result<TOk, TError> OnError(Action<TError> onerror)
		{
			if (this is Error<TOk, TError> err) onerror(err);
			return this;
		}

		public TOk Expect<TException>(string errorMessage) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), errorMessage)
			};
		}
		
		public TOutput Expect<TException, TOutput>(Func<TOk, TOutput> mapper, string errorMessage) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => mapper(ok),
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), errorMessage)
			};
		}
		
		public TOk Expect<TException>(Func<TException> exception) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw exception()
			};
		}
		
		public TOutput Expect<TException, TOutput>(Func<TOk, TOutput> mapper, Func<TException> exception) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => mapper(ok),
				_ => throw exception()
			};
		}

		public Result<TOkOutput, TError> MapTry<TOkOutput>(Func<TOk, TOkOutput> Try, Func<Exception, TError> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (Exception e)
			{
				return Catch(e);
			}
		}

		public Result<TOkOutput, TError> BindTry<TOkOutput>(Func<TOk, Result<TOkOutput, TError>> Try, 
			Func<Exception, Result<TOkOutput, TError>> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (Exception e)
			{
				return Catch(e);
			}
		}
		
		public Result<TOkOutput, TError> MapTry<TOkOutput, TException>(Func<TOk, TOkOutput> Try, Func<TException, TError> Catch)
			where TException: Exception
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (TException e)
			{
				return Catch(e);
			}
		}

		public Result<TOkOutput, TError> BindTry<TOkOutput, TException>(Func<TOk, Result<TOkOutput, TError>> Try, 
			Func<Exception, Result<TOkOutput, TError>> Catch)
			where TException: Exception
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (TException e)
			{
				return Catch(e);
			}
		}

		public void Try<TException>(Action<Result<TOk, TError>> Try, Action<TException> Catch) where TException: Exception
		{
			try
			{
				Try(this);
			}
			catch (TException e)
			{
				Catch(e);
			}
		}

		public TOutput Try<TException, TOutput>(Func<Result<TOk, TError>, TOutput> Try, Func<TException, TOutput> Catch) where TException : 
		Exception
		{
			try
			{
				return Try(this);
			}
			catch (TException e)
			{
				return Catch(e);
			}
		}

		#endregion
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
	}
}