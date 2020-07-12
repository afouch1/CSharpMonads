using System;
using System.Threading.Tasks;

namespace Monads
{
	public class Option<T>
	{
		public static implicit operator Option<T>(T obj) => Option.Some(obj);

		internal Option() { }

		public async Task<Option<TR>> MapAsync<TR>(Func<T, Task<TR>> mapper)
		{
			return this switch
			{
				Some<T> s => await mapper(s),
				_ => Option.None<TR>()
			};
		}
		
		public Option<TR> Map<TR>(Func<T, TR> pred)
		{
			return this switch
			{
				Some<T> s => pred(s.Value),
				_ => Option.None<TR>()
			};
		}

		public async Task<Option<TR>> BindAsync<TR>(Func<T, Task<Option<TR>>> binder)
		{
			return this switch
			{
				Some<T> s => await binder(s),
				_ => Option.None<TR>()
			};
		}

		public Option<TR> Bind<TR>(Func<T, Option<TR>> pred)
		{
			return this switch
			{
				Some<T> s => pred(s.Value),
				_ => Option.None<TR>()
			};
		}

		public Option<T> OnSomeTry(Action<T> Try, Action<Exception> Catch)
		{
			if (this is None<T>) return this;
			try
			{
				var s = (Some<T>) this;
				Try(s);
			}
			catch (Exception e)
			{
				Catch(e);
			}

			return this;
		}
		
		public Option<TOutput> OnSomeTry<TOutput>(Func<T, TOutput> Try, Func<Exception, TOutput> Catch)
		{
			if (this is None<T>) return Option.None<TOutput>();
			try
			{
				var s = (Some<T>) this;
				return Try(s);
			}
			catch (Exception e)
			{
				return Catch(e);
			}
		}
		
		public Option<TOutput> OnSomeTry<TOutput>(Func<T, TOutput> Try, Action<Exception> Catch)
		{
			if (this is None<T>) return Option.None<TOutput>();
			try
			{
				var s = (Some<T>) this;
				return Try(s);
			}
			catch (Exception e)
			{
				Catch(e);
				return Option.None<TOutput>();
			}
		}

		public Option<T> OnSomeTry<TException>(Action<T> Try, Action<TException> Catch) where TException: Exception
		{
			if (this is None<T>) return this;
			try
			{
				var s = (Some<T>) this;
				Try(s);
			}
			catch (TException e)
			{
				Catch(e);
			}

			return this;
		}

		public async Task<Option<T>> OnSomeAsync(Func<T, Task> onsome)
		{
			if (this is Some<T> s) await onsome(s);
			return this;
		}
		
		public Option<T> OnSome(Action<T> onsome)
		{
			if (this is Some<T> s) onsome(s);
			return this;
		}

		public async Task<Option<T>> OnNoneAsync(Func<Task> onnone)
		{
			if (this is None<T>) await onnone();
			return this;
		}
		
		public Option<T> OnNone(Action onnone)
		{
			if (this is None<T>) onnone();
			return this;
		}

		public T Expect<TException>(string message) where TException: Exception
		{
			return this switch
			{
				Some<T> s => s,
				_ => throw (Exception) Activator.CreateInstance(typeof(TException), message)
			};
		}
	}

	public sealed class Some<T> : Option<T>
	{
		internal Some(T value) =>
			Value = value;

		internal T Value { get; }

		public override string ToString() => Value.ToString();
		
		public static implicit operator T(Some<T> s) => s.Value;
	}
	
	public sealed class None<T> : Option<T>
	{
		internal None() { }

		public override string ToString() => "[None]";
	}

	public static class Option
	{
		public static Option<T> Some<T>(T obj) => obj is null ? None<T>() : new Some<T>(obj);

		public static Option<T> None<T>() => new None<T>();

		public static T IfNone<T>(this Option<T> op, T replacement) => op is Some<T> s ? s : replacement;

        public static Option<T> ToOption<T>(this T obj) => obj;

        public static async Task<Option<TR>> MapAsync<TR, T>(
	        this Task<Option<T>> op,
	        Func<T, Task<TR>> mapper)
        {
	        return await op switch
	        {
		        Some<T> s => await mapper(s),
		        _ => None<TR>()
	        };
        }

        public static async Task<Option<TR>> BindAsync<T, TR>(
	        this Task<Option<T>> op,
	        Func<T, Task<Option<TR>>> binder)
        {
	        return await op switch
	        {
		        Some<T> s => await binder(s),
		        _ => None<TR>()
	        };
        }
    }
}