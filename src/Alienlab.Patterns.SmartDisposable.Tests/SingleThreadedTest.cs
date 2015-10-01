namespace Alienlab.Patterns
{
  using System.Threading;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class SingleThreadedTest
  {
    [TestMethod]
    public void Test()
    {
      var owner = new Owner();

      // first attempt - should be the same
      var context1 = owner.CreateContext();
      var contextA = context1;
      Assert.AreEqual(contextA, context1); // obviously

      Assert.IsFalse(contextA.Disposed);
      contextA.AddUser();
      Assert.IsFalse(contextA.Disposed);
      
      // second attempt - should be the same
      var context2 = owner.CreateContext();
      Assert.AreEqual(contextA, context2);

      Assert.IsFalse(contextA.Disposed);
      contextA.AddUser();
      Assert.IsFalse(contextA.Disposed);

      // third attempt - should be the same
      var context3 = owner.CreateContext();
      Assert.AreEqual(contextA, context3);

      Assert.IsFalse(contextA.Disposed);
      contextA.AddUser();
      Assert.IsTrue(contextA.Disposed); // disposed!

      // fourth attempt - should be new!
      var context4 = owner.CreateContext();
      Assert.AreNotEqual(contextA, context4);

      var contextB = context4;
      Assert.AreEqual(contextB, context4); // obviously

      Assert.IsFalse(contextB.Disposed);
      contextB.AddUser();
      Assert.IsFalse(contextB.Disposed);

      // fifth attempt - should be the same as fourth
      var context5 = owner.CreateContext();
      Assert.AreEqual(contextB, context5);

      Assert.IsFalse(contextB.Disposed);
      context5.AddUser();
      Assert.IsFalse(contextB.Disposed);

      // sixth attempt - should be the same as fifth
      var context6 = owner.CreateContext();
      Assert.AreEqual(contextB, context6);

      Assert.IsFalse(contextB.Disposed);
      context6.AddUser();
      Assert.IsTrue(contextB.Disposed); // disposed!
    }

    private class Context : SmartDisposable
    {
      private int UsersCount;

      public Context(SmartDisposableOwner owner) : base(owner)
      {
      }

      internal bool Disposed { get; private set; }

      public void AddUser()
      {
        Interlocked.Increment(ref this.UsersCount);

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

    private class Owner : SmartDisposableOwner
    {
      public Context CreateContext()
      {
        return (Context)this.GetOrCreateSmartDispoable();
      }

      protected override SmartDisposable CreateSmartDisposable()
      {
        return new Context(this);
      }
    }
  }
}
