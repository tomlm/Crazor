using Crazor;

namespace Crazor.Blazor.Tests
{

    [TestClass]
    public class CardRouteTests
    {
        [TestMethod]
        public void ParseDefault()
        {
            string uri = "/Cards/Test";
            var route = CardRoute.Parse(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri, route.Route);
            Assert.AreEqual("", route.Path);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("Default", route.View);
            Assert.AreEqual(0, route.RouteData.Properties().Count());
            Assert.AreEqual(0, route.QueryData.Properties().Count());
        }

        [TestMethod]
        public void ParsePath()
        {
            string uri = "/Cards/Test/View";
            var route = CardRoute.Parse(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri, route.Route);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("View", route.Path);
            Assert.AreEqual("View", route.View);
            Assert.AreEqual(0, route.RouteData.Properties().Count());
            Assert.AreEqual(0, route.QueryData.Properties().Count());
        }

        [TestMethod]
        public void ParseSubPath()
        {
            string uri = "/Cards/Test/View/SubPath";
            var route = CardRoute.Parse(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri, route.Route);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("View/SubPath", route.Path);
            Assert.AreEqual("View", route.View);
            Assert.AreEqual(0, route.RouteData.Properties().Count());
            Assert.AreEqual(0, route.QueryData.Properties().Count());
        }

        [TestMethod]
        public void ParseQuery()
        {
            string uri = "/Cards/Test/View/SubPath?x=1&y=test";
            var route = CardRoute.Parse(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri, route.Route);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("View/SubPath", route.Path);
            Assert.AreEqual("View", route.View);
            Assert.AreEqual(1, (int)route.QueryData["x"]);
            Assert.AreEqual("test", (string)route.QueryData["y"]);
        }


        [TestMethod]
        public void ParseQueryAndRoute()
        {
            string uri = "/Cards/Test/View/SubPath?x=1&y=test";
            var route = CardRoute.Parse(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri, route.Route);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("View/SubPath", route.Path);
            Assert.AreEqual("View", route.View);
            Assert.AreEqual(1, (int)route.QueryData["x"]);
            Assert.AreEqual("test", (string)route.QueryData["y"]);
        }


        [TestMethod]
        public void FromUri()
        {
            var uri = new Uri(new Uri("http://localhost"), "/Cards/Test/View");
            var route = CardRoute.FromUri(uri);
            Assert.IsNotNull(route);
            Assert.AreEqual(uri.AbsolutePath, route.Route);
            Assert.AreEqual("Test", route.App);
            Assert.AreEqual("View", route.Path);
            Assert.AreEqual("View", route.View);
            Assert.AreEqual(0, route.RouteData.Properties().Count());
            Assert.AreEqual(0, route.QueryData.Properties().Count());
        }

    }
}