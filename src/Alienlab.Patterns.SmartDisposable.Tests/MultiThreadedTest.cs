namespace Alienlab.Patterns
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class MultiThreadedTest
  {
    private const int LoopsCount = 100;

    private readonly int ThreadsCount = Environment.ProcessorCount * 10;

    [TestMethod]
    public void Test()
    {
      var owner = new TestSmartDisposableOwner();
      var count = 0;
      var done = 0;
      Exception exception = null;
      for (var i = 0; i < this.ThreadsCount; ++i)
      {
        new Thread(() =>
        {
          try
          {
            for (var j = 0; j < LoopsCount; ++j)
            {
              Interlocked.Increment(ref count);
              using (var context = owner.CreateContext())
              {
                context.AddUser();
              }
            }
          }
          catch (Exception ex)
          {
            exception = ex;

            return;
          }

          Interlocked.Increment(ref done);
        }).Start();
      }

      var deadline = DateTime.UtcNow.AddSeconds(5);
      while (DateTime.UtcNow <= deadline)
      {
        if (exception != null)
        {
          throw new InvalidOperationException("Error", exception);
        }

        if (done < this.ThreadsCount)
        {
          Thread.Sleep(100);

          continue;
        }

        Assert.AreEqual(owner.Count, 10);

        foreach (var smartDisposable in owner.History)
        {
          Assert.IsTrue(smartDisposable.Disposed);
        }

        return;
      }

      Assert.Fail("Timed out");
    }

    private class Context : SmartDisposable, IDisposable
    {
      private int UsersCount;

      public Context(SmartDisposableOwner owner)
        : base(owner, new TimeSpan(0, 0, 2))
      {
      }

      internal bool Disposed { get; private set; }

      public void AddUser()
      {
        Interlocked.Increment(ref this.UsersCount);
      }

      public void Dispose()
      {
        this.Release();
        this.TryDispose();
      }

      protected override bool CanStartDisposal()
      {
        var num = Environment.ProcessorCount * LoopsCount;
        return Interlocked.CompareExchange(ref this.UsersCount, num, num) == num;
      }

      protected override void OnDisposed()
      {
        this.Disposed = true;
      }
    }

    private class TestSmartDisposableOwner : SmartDisposableOwner
    {
      internal readonly List<Context> History = new List<Context>(); 

      internal int Count
      {
        get
        {
          return this.History.Count;
        }
      }

      public Context CreateContext()
      {
        return (Context)this.GetOrCreateSmartDispoable();
      }

      protected override SmartDisposable CreateSmartDisposable()
      {
        var smartDisposable = new Context(this);

        lock (this)
        {
          this.History.Add(smartDisposable);
        }

        return smartDisposable;
      }      
    }
  }
}
