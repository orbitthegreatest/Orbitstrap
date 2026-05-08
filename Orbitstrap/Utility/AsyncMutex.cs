using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orbitstrap.Utility;

public sealed class AsyncMutex : IAsyncDisposable
{
	private readonly bool _initiallyOwned;

	private readonly string _name;

	private Task? _mutexTask;

	private ManualResetEventSlim? _releaseEvent;

	private CancellationTokenSource? _cancellationTokenSource;

	public AsyncMutex(bool initiallyOwned, string name)
	{
		_initiallyOwned = initiallyOwned;
		_name = name;
	}

	public Task AcquireAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
		_releaseEvent = new ManualResetEventSlim();
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_mutexTask = Task.Factory.StartNew(delegate
		{
			try
			{
				CancellationToken token = _cancellationTokenSource.Token;
				using Mutex mutex = new Mutex(_initiallyOwned, _name);
				try
				{
					if (WaitHandle.WaitAny(new WaitHandle[2] { mutex, token.WaitHandle }) != 0)
					{
						taskCompletionSource.SetCanceled(token);
						return;
					}
				}
				catch (AbandonedMutexException)
				{
				}
				taskCompletionSource.SetResult();
				_releaseEvent.Wait();
				mutex.ReleaseMutex();
			}
			catch (OperationCanceledException)
			{
				taskCompletionSource.TrySetCanceled(cancellationToken);
			}
			catch (Exception exception)
			{
				taskCompletionSource.TrySetException(exception);
			}
		}, null, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		return taskCompletionSource.Task;
	}

	public async Task ReleaseAsync()
	{
		_releaseEvent?.Set();
		if (_mutexTask != null)
		{
			await _mutexTask;
		}
	}

	public async ValueTask DisposeAsync()
	{
		_cancellationTokenSource?.Cancel();
		await ReleaseAsync();
		_releaseEvent?.Dispose();
		_cancellationTokenSource?.Dispose();
	}
}
