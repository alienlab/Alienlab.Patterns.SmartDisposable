namespace Alienlab.Patterns
{
  using System;
  using System.Threading;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class SingleThreadedTest
  {
    [TestMethod]
    public void Test()
    {
      var owner = new Owner();

      Context contextA;

      // first attempt - should be the same
      using (var context1 = owner.CreateContext())
      {
        contextA = context1;
        Assert.AreEqual(contextA, context1); // obviously

        Assert.IsFalse(contextA.Disposed);
        contextA.AddUser();
        Assert.IsFalse(contextA.Disposed);
      }

      Assert.IsFalse(contextA.Disposed);

      // second attempt - should be the same
      using (var context2 = owner.CreateContext())
      {
        Assert.AreEqual(contextA, context2);

        Assert.IsFalse(contextA.Disposed);
        contextA.AddUser();
        Assert.IsFalse(contextA.Disposed);
      }

      Assert.IsFalse(contextA.Disposed);

      // third attempt - should be the same
      using (var context3 = owner.CreateContext())
      {
        Assert.AreEqual(contextA, context3);

        Assert.IsFalse(contextA.Disposed);
        contextA.AddUser();
        Assert.IsFalse(contextA.Disposed);
      }

      Assert.IsTrue(contextA.Disposed); // disposed!

      Context contextB;

      // fourth attempt - should be new!
      using (var context4 = owner.CreateContext())
      {
        Assert.AreNotEqual(contextA, context4);

        contextB = context4;
        Assert.AreEqual(contextB, context4); // obviously

        Assert.IsFalse(contextB.Disposed);
        contextB.AddUser();
        Assert.IsFalse(contextB.Disposed);
      }

      Assert.IsFalse(contextB.Disposed);

      // fifth attempt - should be the same as fourth
      using (var context5 = owner.CreateContext())
      {
        Assert.AreEqual(contextB, context5);

        Assert.IsFalse(contextB.Disposed);
        contextB.AddUser();
        Assert.IsFalse(contextB.Disposed);
      }

      // sixth attempt - should be the same as fifth
      using (var context6 = owner.CreateContext())
      {
        Assert.AreEqual(contextB, context6);

        Assert.IsFalse(contextB.Disposed);
        contextB.AddUser();
        Assert.IsFalse(contextB.Disposed);
      }

      Assert.IsTrue(contextB.Disposed); // disposed!
    }

    internal class Context : SmartDisposable, IDisposable
    {
      private int UsersCount;

      public Context(SmartDisposableOwner owner)
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

    internal class Owner : SmartDisposableOwner
    {
      internal Context CreateContext()
      {
        return (Context)this.GetOrCreateSmartDispoable();
      }

      protected override SmartDisposable CreateSmartDisposable()
      {
        return new Context(this);
      }

      protected override void LogError(string message)
      {
        throw new InvalidOperationException(message);
      }
    }
  }
}
