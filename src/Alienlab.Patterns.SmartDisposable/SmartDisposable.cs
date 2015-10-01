namespace Alienlab.Patterns
{
  using System.Diagnostics;
  using System.Threading;

  public abstract class SmartDisposable 
  {
    private readonly SmartDisposableOwner Owner;

    private int HoldersCounter;

    private int ExpectedCommitsCounter;

    private bool IsDisposed;

    protected SmartDisposable(SmartDisposableOwner owner)
    {
      this.Owner = owner;
    }

    internal SmartDisposable IncrementUsageCounter()
    {
      Interlocked.Increment(ref this.ExpectedCommitsCounter);

      return this;
    }

    protected abstract bool CanStartDisposal();

    protected abstract void OnDisposed();

    protected virtual void LogError(string message)
    {
      Debug.WriteLine(message);
    }

    protected void TryDispose()
    {
      if (!this.CanStartDisposal())
      {
        return;
      }

      // disposing has started so we need to prevent this instance from being obtained in new places
      this.Owner.InvalidateCache(this);

      if (this.HoldersCounter < 0 || this.ExpectedCommitsCounter < 0)
      {
        // correct implementation of this library must make this situation impossible
        this.LogError(string.Format("[IntervalCommitLuceneUpdateContext] One of counters went below zero. Holders: {0}, Commits: {1}", this.HoldersCounter, this.ExpectedCommitsCounter));

        return;
      }
      
      // this check must be first as it is set first inside lock - important for best performance
      if (this.IsDisposed)
      {
        // already disposed, nothing to do then
        return;
      }

      if (this.HoldersCounter != 0 && this.ExpectedCommitsCounter != 0)
      {
        // still is in use, must be disposed later
        return;
      }

      lock (this)
      {
        if (this.IsDisposed)
        {
          // already disposed (probably by another thread), nothing to do then
          return;
        }

        this.IsDisposed = true;

        Interlocked.Decrement(ref this.HoldersCounter);
      }

      this.OnDisposed();
    }
  }
}
