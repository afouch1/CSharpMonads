using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Monads
{
	/// <summary>
	/// Base Result class encapsulating either a success value of TOk or error value of TError
	/// </summary>
	public class Result<TOk, TError>
	{
		internal Result() { }

		public static implicit operator Result<TOk, TError>(TOk obj) => Result.Ok<TOk, TError>(obj);

		public static implicit operator Result<TOk, TError>(TError obj) => Result.Error<TOk, TError>(obj);
		
		#region Higher Order Functions
		
		/// <summary>
		/// Maps the success result to a new value asynchronously.
		/// </summary>
		public async Task<Result<TOutput, TError>> MapAsync<TOutput>(Func<TOk, Task<TOutput>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => await pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}
		
		/// <summary>
		/// Maps the success result to a new value
		/// </summary>
		public Result<TOutput, TError> Map<TOutput>(Func<TOk, TOutput> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}

		/// <summary>
		/// Binds the result value to a new result value asynchronously 
		/// </summary>
		public async Task<Result<TOutput, TError>> BindAsync<TOutput>(
			Func<Result<TOk, TError>, Task<Result<TOutput, TError>>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => await pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}
		
		/// <summary>
		/// Binds teh result value to a new result value
		/// </summary>
		public Result<TOutput, TError> Bind<TOutput>(Func<TOk, Result<TOutput, TError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}

		/// <summary>
		/// Binds the possible error value with a new error value
		/// </summary>
		public Result<TOk, TOutError> BindError<TOutError>(Func<TError, Result<TOk, TOutError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => Result.Ok<TOk, TOutError>(ok),
				Error<TOk, TError> err => pred(err)
			};
		}

		/// <summary>
		/// Performs an asynchronous action if the result is a success.
		/// Returns the input Result.
		/// </summary>
		public async Task<Result<TOk, TError>> OnSuccessAsync(Func<TOk, Task> onsuccess)
		{
			if (this is Ok<TOk, TError> ok) await onsuccess(ok);
			return this;
		}

		/// <summary>
		/// Performs an action if it is a success.
		/// Returns the input Result.
		/// </summary>
		public Result<TOk, TError> OnSuccess(Action<TOk> onsuccess)
		{
			if (this is Ok<TOk, TError> ok) onsuccess(ok);
			return this;
		}

		/// <summary>
		/// Performs an action if the result is an error.
		/// Returns the input Result.
		/// </summary>
		public Result<TOk, TError> OnError(Action<TError> onerror)
		{
			if (this is Error<TOk, TError> err) onerror(err);
			return this;
		}

		/// <summary>
		/// Performs an asynchronous action if the result is an error.
		/// Returns the input result 
		/// </summary>
		public async Task<Result<TOk, TError>> OnErrorAsync(Func<TError, Task> onerror)
		{
			if (this is Error<TOk, TError> err) await onerror(err);
			return this;
		}

		/// <summary>
		/// Forces the acquisition of a successful Result. Throws an error of type TException
		/// with an error message if the result is an error. 
		/// </summary>
		public TOk Expect<TException>(string errorMessage) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), errorMessage)
			};
		}
		
		/// <summary>
		/// Forces the acquisition of a successful Result and performs an action on it.
		/// Throws an error of type TException with an error message if the result is an error.
		/// </summary>
		public TOutput Expect<TException, TOutput>(Func<TOk, TOutput> mapper, string errorMessage) where TException : Exception
		{
			return this switch
			{
				Ok<TOk, TError> ok => mapper(ok),
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), errorMessage)
			};
		}
		
		/// <summary>
		/// Forces the acquisition of a successful Result and performs an action on it.
		/// </summary>
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

		/// <summary>
		/// Maps the successful result to a new value and catches a all exceptions.
		/// Exceptions are handled by the "catch" parameter.
		/// </summary>
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

		/// <summary>
		/// Binds the successful result to a new result and catches all exceptions.
		/// Excepts are handled by the "catch" parameter.
		/// </summary>
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

		public static Result<TOk, TError> Try<TOk, TError>(Func<TOk> throwable, Func<Exception, TError> handler)
		{
			try
			{
				return throwable();
			}
			catch (Exception e)
			{
				return handler(e);
			}
		}

		public static async Task<Result<TOk, TError>> TryAsync<TOk, TError>(Func<Task<TOk>> throwable,
			Func<Exception, Task<TError>> handler)
		{
			try
			{
				return await throwable();
			}
			catch (Exception e)
			{
				return await handler(e);
			}
		}

		public static Result<TOk, TError> Try<TOk, TError>(Func<Result<TOk, TError>> throwable,
			Func<Exception, Result<TOk, TError>> handler)
		{
			try
			{
				return throwable();
			}
			catch (Exception e)
			{
				return handler(e);
			}
		}

		public static async Task<Result<TOk, TError>> TryAsync<TOk, TError>(
			Func<Task<Result<TOk, TError>>> throwable,
			Func<Exception, Task<Result<TOk, TError>>> handler)
		{
			try
			{
				return await throwable();
			}
			catch (Exception e)
			{
				return await handler(e);
			}
		}

		public static async Task<Result<TOutput, TError>> MapAsync<TInput, TOutput, TError>(
			this Task<Result<TInput, TError>> res,
			Func<TInput, Task<TOutput>> pred)
		{
			return await res switch
			{
				Ok<TInput, TError> ok => await pred(ok),
				Error<TInput, TError> err => Error<TOutput, TError>(err)
			};
		}

		public static async Task<Result<TOutput, TError>> BindAsync<TInput, TOutput, TError>(
			this Task<Result<TInput, TError>> res,
			Func<Result<TInput, TError>, Task<Result<TOutput, TError>>> pred)
		{
			return await res switch
			{
				Ok<TInput, TError> ok => await pred(ok),
				Error<TInput, TError> err => Error<TOutput, TError>(err)
			};
		}
	}
}