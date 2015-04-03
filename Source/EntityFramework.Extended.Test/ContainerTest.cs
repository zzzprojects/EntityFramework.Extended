using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFramework.Test
{
    
    public class ContainerTest
    {
        [Fact]
        public void RegisterResolve()
        {
            var c = new Container();
            c.Register<ITest1>(() => new Test1());

            var t1 = c.Resolve<ITest1>();
            Assert.NotNull(t1);
            Assert.IsType(typeof(Test1), t1);

            c.Register<ITest2>(() => new Test2());

            var t2 = c.Resolve<ITest2>();
            Assert.NotNull(t1);
            Assert.IsType(typeof(Test2), t2);
        }

        [Fact]
        public void RegisterResolveArguments()
        {
            var c = new Container();
            c.Register<ITest1>(() => new Test1());

            var t1 = c.Resolve<ITest1>();
            Assert.NotNull(t1);
            Assert.IsType(typeof(Test1), t1);

            c.Register<ITest2>(() => new Test2());

            var t2 = c.Resolve<ITest2>();
            Assert.NotNull(t2);
            Assert.IsType(typeof(Test2), t2);

            c.Register<ITest3, ITest1, ITest2>((a1, a2) => new Test3(a1, a2));

            var t3 = c.Resolve<ITest3>();
            Assert.NotNull(t3);
            Assert.IsType(typeof(Test3), t3);

            Assert.NotNull(t3.Test1);
            Assert.IsType(typeof(Test1), t3.Test1);

            Assert.NotNull(t3.Test2);
            Assert.IsType(typeof(Test2), t3.Test2);
        }


        interface ITest1
        {
            string A { get; set; }
        }

        interface ITest2
        {
            string B { get; set; }
        }

        interface ITest3
        {
            string C { get; set; }
            ITest1 Test1 { get; }
            ITest2 Test2 { get; }
        }

        class Test1 : ITest1
        {
            public string A { get; set; }
            public string Key { get; set; }
        }
        class Test2 : ITest2
        {
            public string B { get; set; }
            public string Key { get; set; }
        }

        class Test3 : ITest3
        {
            private ITest1 _test1;
            private ITest2 _test2;

            public Test3(ITest1 test1, ITest2 test2)
            {
                _test1 = test1;
                _test2 = test2;
            }

            public ITest1 Test1
            {
                get { return _test1; }
            }

            public ITest2 Test2
            {
                get { return _test2; }
            }

            public string C { get; set; }
            public string Key { get; set; }

        }
    }
}
