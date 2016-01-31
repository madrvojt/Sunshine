using NUnit.Framework;

namespace Sunshine.Tests.Tests
{
    [TestFixture]
    public class TestPractice
    {

        [Test]
        public void TestThatDemonstratesAssertions()
        {
            const int a = 5;
            const int b = 3;
            const int c = 5;
            const int d = 10;

            Assert.AreEqual(a, c);
            Assert.True(d > a, "Y should be true");
            Assert.False(a == b, "Z should be false");

            if (b > d)
            {
                Assert.Fail("XX should never happen");
            }
        }
    }
}

