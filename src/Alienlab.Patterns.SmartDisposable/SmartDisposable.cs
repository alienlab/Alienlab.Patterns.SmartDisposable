namespace Alienlab.Patterns
{
  using System.Threading;

  public abstract class SmartDisposable
  {
    private readonly SmartDisposableOwner Owner;

    private int HoldersCounter;

    private bool IsDisposed;

    protected SmartDisposable(SmartDisposableOwner owner)
    {
      this.Owner = owner;
    }

    internal SmartDisposable IncrementUsageCounter()
    {
      Interlocked.Increment(ref this.HoldersCounter);

      return this;
    }

    protected abstract bool CanStartDisposal();

    protected abstract void OnDisposed();
    
    /// <summary>
    /// Checks and disposes if it is right time to do so (according to this.CanStartDisposal).
    /// </summary>
    protected void TryDispose()
    {
      Interlocked.Decrement(ref this.HoldersCounter);

      if (!this.CanStartDisposal())
      {
        return;
      }

      // disposing has started so we need to prevent this instance from being obtained in new places
      this.Owner.InvalidateCache(this);

      if (this.HoldersCounter < 0)
      {
        // correct implementation of this library must make this situation impossible
        this.Owner.LogError(string.Format("Holders counter went below zero: {0}", this.HoldersCounter));

        return;
      }
      
      // this check must be first as it is set first inside lock - important for best performance
      if (this.IsDisposed)
      {
        // already disposed, nothing to do then
        return;
      }

      if (Interlocked.CompareExchange(ref this.HoldersCounter, 0, 0) != 0)
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
      }

      this.OnDisposed();
    }
  }
}
