namespace Alienlab.Patterns
{
  using System;
  using System.Diagnostics;
  using System.Threading;

  public abstract class SmartDisposable
  {
    private readonly SmartDisposableOwner Owner;

    private readonly Stopwatch DisposalDelay;

    private readonly TimeSpan MaxDisposalDelay;

    private int HoldersCounter;

    private bool IsDisposed;

    protected SmartDisposable(SmartDisposableOwner owner, TimeSpan maxDisposalDelay)
    {
      this.Owner = owner;
      this.MaxDisposalDelay = maxDisposalDelay;

      this.DisposalDelay = new Stopwatch();
    }

    internal SmartDisposable IncrementUsageCounter()
    {
      Interlocked.Increment(ref this.HoldersCounter);

      return this;
    }

    #region Protected Abstract Methods

    protected abstract bool CanStartDisposal();

    protected abstract void OnDisposed();

    #endregion

    protected void Release()
    {
      Interlocked.Decrement(ref this.HoldersCounter);
    }

    /// <summary>
    /// Checks and disposes if it is right time to do so (according to this.CanStartDisposal).
    /// </summary>
    protected void TryDispose()
    {
      if (!this.CanStartDisposal())
      {
        return;
      }

      // disposing has started so we need to prevent this instance from being obtained in new places
      this.Owner.InvalidateCache(this);
      
      // this check must be first as it is set first inside lock - important for best performance
      if (this.IsDisposed)
      {
        // already disposed, nothing to do then
        return;
      }

      var holdersCount = Interlocked.CompareExchange(ref this.HoldersCounter, 0, 0);
      var disposalDelay = this.DisposalDelay.Elapsed;
      if (holdersCount > 0 && disposalDelay < this.MaxDisposalDelay)
      {
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
      }

      this.OnDisposed();
    }
  }
}
