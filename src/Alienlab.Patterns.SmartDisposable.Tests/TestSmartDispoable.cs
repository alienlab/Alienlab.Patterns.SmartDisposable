namespace Alienlab.Patterns
{
  using System;
  using System.Threading;

  internal class TestSmartDispoable : SmartDisposable, IDisposable
  {
    private int UsersCount;

    public TestSmartDispoable(SmartDisposableOwner owner)
      : base(owner)
    {
    }

    internal bool Disposed { get; private set; }

    public void AddUser()
    {
      Interlocked.Increment(ref this.UsersCount);
    }

    public void Dispose()
    {
      this.TryDispose();
    }

    protected override bool CanStartDisposal()
    {
      return this.UsersCount >= 3;
    }

    protected override void OnDisposed()
    {
      this.Disposed = true;
    }
  }
}