using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Chef.FastMember.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            object member = new Member();

            var accessor = TypeAccessor.Create(member.GetType());

            //System.Console.WriteLine($"ABC={accessor[member, "ABC"]}");
            //System.Console.WriteLine($"Abc={accessor[member, "Abc"]}");
            //System.Console.WriteLine($"AbC={accessor[member, "AbC"]}");
            //System.Console.WriteLine($"abc={accessor[member, "abc"]}");

            //accessor[member, "ABC"] = 2;
            //accessor[member, "Abc"] = 23;
            //accessor[member, "AbC"] = 234;
            //accessor[member, "abc"] = 2345;

            //System.Console.WriteLine();
            //System.Console.WriteLine($"ABC={accessor[member, "ABC"]}");
            //System.Console.WriteLine($"Abc={accessor[member, "Abc"]}");
            //System.Console.WriteLine($"AbC={accessor[member, "AbC"]}");
            //System.Console.WriteLine($"abc={accessor[member, "abc"]}");

            //System.Console.WriteLine();
            //accessor.Invoke(member, "MyPublicVoid");
            //accessor.Invoke(member, "MyPrivateVoid");

            //System.Console.WriteLine();
            //System.Console.WriteLine($"MyPublicId={accessor.Invoke(member, "MyPublicId")}");
            //System.Console.WriteLine($"MyPrivateId={accessor.Invoke(member, "MyPrivateId")}");
            //System.Console.WriteLine();
            //System.Console.WriteLine($"MyPublicAdd(1, 2)={accessor.Invoke(member, "MyPublicAdd", 1, 2)}");
            //System.Console.WriteLine($"MyPrivateAdd(3, 4)={accessor.Invoke(member, "MyPrivateAdd", 3, 4)}");

            //System.Console.Read();
            BenchmarkRunner.Run<MemberGettersBenchmark>();
        }
    }

    public class MemberGettersBenchmark
    {
        private readonly TypeAccessor accessor;
        private readonly PropertyInfo property;
        private readonly FieldInfo field;
        private readonly MethodInfo method;
        private readonly Member member;

        public MemberGettersBenchmark()
        {
            this.accessor = TypeAccessor.Create<Member>();
            this.property = typeof(Member).GetProperty("ABC");
            this.field = typeof(Member).GetField("abc", BindingFlags.NonPublic | BindingFlags.Instance);
            this.method = typeof(Member).GetMethod("MyPublicId");
            this.member = new Member();
        }

        [Benchmark]
        public void GetPropertyValueByTypeAccessor()
        {
            var abc = this.accessor[this.member, "ABC"];
        }

        [Benchmark]
        public void GetFieldValueByTypeAccessor()
        {
            var abc = this.accessor[this.member, "abc"];
        }

        [Benchmark]
        public void CallMyPublicIdByTypeAccessor()
        {
            this.accessor.Invoke(this.member, "MyPublicId");
        }

        [Benchmark]
        public void SetPropertyValueByTypeAccessor()
        {
            this.accessor[this.member, "ABC"] = 123;
        }

        [Benchmark]
        public void SetFieldValueByTypeAccessor()
        {
            this.accessor[this.member, "abc"] = 123;
        }

        [Benchmark]
        public void GetPropertyValueByGetValue()
        {
            var abc = this.property.GetValue(this.member);
        }

        [Benchmark]
        public void GetFieldValueByGetValue()
        {
            var abc = this.field.GetValue(this.member);
        }

        [Benchmark]
        public void SetPropertyValueBySetValue()
        {
            this.property.SetValue(this.member, 123);
        }

        [Benchmark]
        public void SetFieldValueBySetValue()
        {
            this.field.SetValue(this.member, 123);
        }

        [Benchmark]
        public void CallMyPublicIdByInvoke()
        {
            this.method.Invoke(this.member, null);
        }
    }

    public class Member
    {
        public int AbC = 11;

        private int abc = 22;

        public int ABC { get; set; } = 1;

        private int Abc { get; set; } = 2;
        
        public void MyPublicVoid()
        {
            System.Console.WriteLine("MyPublicVoid");
        }

        public int MyPublicId()
        {
            return 4;
        }

        public int MyPublicAdd(int a, int b)
        {
            return a + b;
        }

        private void MyPrivateVoid()
        {
            System.Console.WriteLine("MyPrivateVoid");
        }

        private int MyPrivateId()
        {
            return 5;
        }

        private int MyPrivateAdd(int a, int b)
        {
            return a + b;
        }
    }
}