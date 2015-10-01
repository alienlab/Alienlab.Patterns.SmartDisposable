namespace Alienlab.Patterns
{
  using System;
  using System.Diagnostics;

  public abstract class SmartDisposableOwner
  {
    private SmartDisposable Cache;

    /// <summary>
    /// Evicts given instance of SmartDisposable object from cache.
    /// </summary>
    internal void InvalidateCache(SmartDisposable smartDisposable)
    {
      if (this.Cache == smartDisposable)
      {
        this.Cache = null;
      }
    }

    /// <summary>
    /// Gets SmartDisposable object from cache if disposal conditions are not satisfied, otherwise creates new one.
    /// </summary>
    protected SmartDisposable GetOrCreateSmartDispoable()
    {
      var smartDisposable = this.Cache;
      if (smartDisposable != null)
      {
        return smartDisposable.IncrementUsageCounter();
      }

      lock (this)
      {
        smartDisposable = this.Cache;
        if (smartDisposable != null)
        {
          return smartDisposable.IncrementUsageCounter();
        }

        this.Cache = null;

        var newSmartDisposable = this.CreateSmartDisposable();
        if (newSmartDisposable == null)
        {
          throw new InvalidOperationException("The SmartDisposable object was not created");
        }

        this.Cache = newSmartDisposable;

        return newSmartDisposable.IncrementUsageCounter();
      }
    }

    protected abstract SmartDisposable CreateSmartDisposable();
  }
}