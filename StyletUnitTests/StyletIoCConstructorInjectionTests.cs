﻿using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    interface I1 { }

    class C1 : I1 { }
    class C2 : I1
    {
        public C1 C1;
        public C2(C1 c1)
        {
            this.C1 = c1;
        }
    }

    class C3
    {
        public C1 C1;
        public C2 C2;
        public C3(C1 c1, C2 c2)
        {
            this.C1 = c1;
            this.C2 = c2;
        }
    }

    class C4
    {
        public C1 C1;
        public C4([Inject("key1")] C1 c1)
        {
            this.C1 = c1;
        }
    }

    class C5
    {
        public bool RightConstructorCalled;
        public C5(C1 c1, C2 c2 = null, C3 c3 = null, C4 c4 = null)
        {
        }

        public C5(C1 c1, C2 c2, C3 c3 = null)
        {
            this.RightConstructorCalled = true;
        }

        public C5(C1 c1, C2 c2)
        {
        }
    }

    class C6
    {
        public bool RightConstructorCalled;
        [Inject]
        public C6(C1 c1)
        {
            this.RightConstructorCalled = true;
        }

        public C6(C1 c1, C2 c2)
        {
        }
    }

    class C7
    {
        [Inject]
        public C7()
        {
        }

        [Inject]
        public C7(C1 c1)
        {
        }
    }

    class C8
    {
        public IEnumerable<I1> I1s;
        public C8(IEnumerable<I1> i1s)
        {
            this.I1s = i1s;
        }
    }

    [TestFixture]
    public class StyletIoCConstructorInjectionTests
    {
        [Test]
        public void RecursivelyPopulatesConstructorParams()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<C2>().ToSelf();
            ioc.Bind<C3>().ToSelf();

            var c3 = ioc.Get<C3>();

            Assert.IsInstanceOf<C3>(c3);
            Assert.IsInstanceOf<C1>(c3.C1);
            Assert.IsInstanceOf<C2>(c3.C2);
            Assert.IsInstanceOf<C1>(c3.C2.C1);
        }

        [Test]
        public void UsesConstructorParamKeys()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf().WithKey("key1");
            ioc.Bind<C4>().ToSelf();

            var c4 = ioc.Get<C4>();

            Assert.IsInstanceOf<C1>(c4.C1);
        }

        [Test]
        public void ThrowsIfConstructorParamKeyNotRegistered()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C4>().ToSelf();
            ioc.Bind<C1>().ToSelf();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C4>());
        }

        [Test]
        public void ChoosesCtorWithMostParamsWeCanFulfill()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<C2>().ToSelf();
            ioc.Bind<C5>().ToSelf();

            var c5 = ioc.Get<C5>();
            Assert.IsTrue(c5.RightConstructorCalled);
        }

        [Test]
        public void ChoosesCtorWithAttribute()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<C2>().ToSelf();
            ioc.Bind<C6>().ToSelf();

            var c6 = ioc.Get<C6>();
            Assert.IsTrue(c6.RightConstructorCalled);
        }

        [Test]
        public void ThrowsIfMoreThanOneCtorWithAttribute()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().ToSelf();
            ioc.Bind<C7>().ToSelf();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C7>());
        }

        [Test]
        public void ThrowsIfNoCtorAvailable()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C5>().ToSelf();

            Assert.Throws<StyletIoCFindConstructorException>(() => ioc.Get<C5>());
        }

        [Test]
        public void SingletonActuallySingleton()
        {
            var ioc = new StyletIoC();
            ioc.BindSingleton<C1>().ToSelf();
            ioc.Bind<C2>().ToSelf();
            ioc.Bind<C3>().ToSelf();

            var c3 = ioc.Get<C3>();
            Assert.AreEqual(ioc.Get<C1>(), c3.C1);
            Assert.AreEqual(ioc.Get<C2>().C1, c3.C1);
        }

        [Test]
        public void IEnumerableHasAllInjected()
        {
            var ioc = new StyletIoC();
            ioc.Bind<C1>().To<C1>();
            ioc.Bind<I1>().To<C1>();
            ioc.Bind<I1>().To<C2>();
            ioc.Bind<C8>().ToSelf();

            var c8 = ioc.Get<C8>();
            var i1s = c8.I1s.ToList();

            Assert.AreEqual(2, i1s.Count);
            Assert.IsInstanceOf<C1>(i1s[0]);
            Assert.IsInstanceOf<C2>(i1s[1]);
        }
    }
}