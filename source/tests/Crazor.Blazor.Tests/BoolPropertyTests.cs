using Crazor.Blazor.Components;

namespace Crazor.Blazor.Tests
{



    [TestClass]
    public class BoolPropertyTests
    {
        [TestMethod]
        public void Equals()
        {
            Assert.AreEqual((BoolProperty)true, (BoolProperty)true);
            Assert.AreEqual((BoolProperty)false, (BoolProperty)false);
            Assert.AreEqual((BoolProperty)"True", (BoolProperty)true);
            Assert.AreEqual((BoolProperty)"False", (BoolProperty)false);
            Assert.AreEqual((BoolProperty)true, (BoolProperty)"True");
            Assert.AreEqual((BoolProperty)false, (BoolProperty)"False");
            Assert.AreEqual((BoolProperty)"True", (BoolProperty)"True");
            Assert.AreEqual((BoolProperty)"False", (BoolProperty)"False");
        }

        [TestMethod]
        public void AreNotEquals()
        {
            Assert.AreNotEqual((BoolProperty)true, (BoolProperty)false);
            Assert.AreNotEqual((BoolProperty)false, (BoolProperty)true);
            Assert.AreNotEqual((BoolProperty)"True", (BoolProperty)false);
            Assert.AreNotEqual((BoolProperty)"False", (BoolProperty)true);
            Assert.AreNotEqual((BoolProperty)true, (BoolProperty)"False");
            Assert.AreNotEqual((BoolProperty)false, (BoolProperty)"True");
        }

        public void ImplicitComparison()
        {
            BoolProperty t = true;
            BoolProperty f = false;
            BoolProperty T = "True";
            BoolProperty F = "False";

            Assert.AreEqual(t, t);
            Assert.AreEqual(t, T);
            Assert.AreEqual(T, f);
            Assert.AreEqual(T, F);

            Assert.AreNotEqual(t, f);
            Assert.AreNotEqual(t, F);
            Assert.AreNotEqual(T, f);
            Assert.AreNotEqual(T, F);

            Assert.AreNotEqual(f, t);
            Assert.AreNotEqual(f, T);
            Assert.AreNotEqual(F, t);
            Assert.AreNotEqual(F, T);
        }

        public void ToStringTest()
        {
            BoolProperty t = true;
            BoolProperty f = false;
            BoolProperty T = "True";
            BoolProperty F = "False";
            Assert.AreEqual("true", t);
            Assert.AreEqual("true", T);
            Assert.AreEqual("false", f);
            Assert.AreEqual("false", F);
        }
    }
}