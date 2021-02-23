using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chef.FastMember.Tests
{
    [TestClass]
    public class TypeAccessorTest
    {
        [TestMethod]
        public void Test_TypeAccessor_Getter()
        {
            var accessor = TypeAccessor.Create<Member>();

            var member = new Member();

            accessor[member, "AbC"].Should().Be(11);
            accessor[member, "abc"].Should().Be(22);
            accessor[member, "ABC"].Should().Be(1);
            accessor[member, "Abc"].Should().Be(2);
        }

        [TestMethod]
        public void Test_TypeAccessor_Setter()
        {
            var accessor = TypeAccessor.Create<Member>();

            var member = new Member();

            accessor[member, "AbC"] = 111;
            accessor[member, "abc"] = 222;
            accessor[member, "ABC"] = 11;
            accessor[member, "Abc"] = 22;

            accessor[member, "AbC"].Should().Be(111);
            accessor[member, "abc"].Should().Be(222);
            accessor[member, "ABC"].Should().Be(11);
            accessor[member, "Abc"].Should().Be(22);
        }
    }

    public class Member
    {
        public int AbC = 11;

        private int abc = 22;

        public int ABC { get; set; } = 1;

        private int Abc { get; set; } = 2;
    }
}