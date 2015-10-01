namespace Alienlab.Patterns
{
  using System;

  internal class TestSmartDisposableOwner : SmartDisposableOwner
  {
    internal TestSmartDispoable CreateContext()
    {
      return (TestSmartDispoable)this.GetOrCreateSmartDispoable();
    }

    protected override SmartDisposable CreateSmartDisposable()
    {
      return new TestSmartDispoable(this);
    }

    protected override void LogError(string message)
    {
      throw new InvalidOperationException(message);
    }
  }
}