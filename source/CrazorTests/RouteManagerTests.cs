using Crazor;
using Microsoft.AspNetCore.Mvc;

namespace CrazorTests
{

    public class Cards_Route_Default : CardView
    {
        public void OnLoadRoute()
        {
        }
    }

    public class Cards_Route_Static : CardView
    {
    }

    [Route("{value}")]
    public class Cards_Route_Path1 : CardView
    {
        public void OnLoadRoute(string value)
        {
            Assert.AreEqual("test", value);
        }
    }


    [Route("resource/{resourceId}")]
    public class Cards_Route_Path2 : CardView
    {
        public void OnLoadRoute(string resourceId)
        {
            Assert.AreEqual("12345", resourceId);
        }
    }

    [Route("resource/{value}/sub/{subvalue}")]
    public class Cards_Route_Path3 : CardView
    {
        public void OnLoadRoute(string value, string subvalue)
        {
            Assert.AreEqual("1234", value);
            Assert.AreEqual("5678", subvalue);
        }
    }

    [Route("resource/delete")]
    public class Cards_Route_PathWithStatic : CardView
    {
    }

    [Route("optional/{value?}")]
    public class Cards_Route_OptionalPath: CardView
    {
    }


    [TestClass]
    public class RouteManagerTests
    {
        [TestMethod]
        public void RouteManagerMatch()
        {

            RouteManager rm = new RouteManager();
            rm.Add(typeof(Cards_Route_Path1));
            rm.Add(typeof(Cards_Route_Path3));
            rm.Add(typeof(Cards_Route_Path2));
            rm.Add(typeof(Cards_Route_OptionalPath)); 
            rm.Add(typeof(Cards_Route_Default));
            rm.Add(typeof(Cards_Route_PathWithStatic));
            rm.Add(typeof(Cards_Route_Static)); 

            var cardRoute = CardRoute.Parse("/Cards/Route/Static");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out var type));
            Assert.AreEqual(typeof(Cards_Route_Static), type);
            Assert.AreEqual(0, cardRoute.RouteData.Properties().Count());
            Assert.AreEqual(0, cardRoute.QueryData.Properties().Count());

            cardRoute = CardRoute.Parse("/Cards/Route");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Default), type);
            Assert.AreEqual(0, cardRoute.RouteData.Properties().Count());
            Assert.AreEqual(0, cardRoute.QueryData.Properties().Count());

            cardRoute = CardRoute.Parse("/Cards/Route/test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path1), type);
            Assert.AreEqual("test", cardRoute.RouteData["value"]);
            Assert.AreEqual(0, cardRoute.QueryData.Properties().Count());

            cardRoute = CardRoute.Parse("/Cards/Route/resource/1234");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path2), type);
            Assert.AreEqual("1234", cardRoute.RouteData["resourceId"]);
            Assert.AreEqual(0, cardRoute.QueryData.Properties().Count());

            cardRoute = CardRoute.Parse("/Cards/Route/resource/1234/sub/5678");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path3), type);
            Assert.AreEqual("1234", cardRoute.RouteData["value"]);
            Assert.AreEqual("5678", cardRoute.RouteData["subvalue"]);
            Assert.AreEqual(0, cardRoute.QueryData.Properties().Count());

            cardRoute = CardRoute.Parse("/Cards/Route/optional");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_OptionalPath), type);
            Assert.IsFalse(cardRoute.RouteData.ContainsKey("value"));

            cardRoute = CardRoute.Parse("/Cards/Route/optional/foo");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_OptionalPath), type);
            Assert.AreEqual("foo", cardRoute.RouteData["value"]);
        }

        [TestMethod]
        public void RouteManagerMatchQuery()
        {

            RouteManager rm = new RouteManager();
            rm.Add(typeof(Cards_Route_Path1));
            rm.Add(typeof(Cards_Route_Path3));
            rm.Add(typeof(Cards_Route_Path2));
            rm.Add(typeof(Cards_Route_Default));
            rm.Add(typeof(Cards_Route_PathWithStatic));
            rm.Add(typeof(Cards_Route_Static));

            var cardRoute = CardRoute.Parse("/Cards/Route/Static?x=15&y=test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out var type));
            Assert.AreEqual(typeof(Cards_Route_Static), type);
            Assert.AreEqual("15", cardRoute.QueryData["x"]);
            Assert.AreEqual("test", cardRoute.QueryData["y"]);

            cardRoute = CardRoute.Parse("/Cards/Route?x=15&y=test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Default), type);
            Assert.AreEqual("15", cardRoute.QueryData["x"]);
            Assert.AreEqual("test", cardRoute.QueryData["y"]);

            cardRoute = CardRoute.Parse("/Cards/Route/test?x=15&y=test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path1), type);
            Assert.AreEqual("test", cardRoute.RouteData["value"]);
            Assert.AreEqual("15", cardRoute.QueryData["x"]);
            Assert.AreEqual("test", cardRoute.QueryData["y"]);

            cardRoute = CardRoute.Parse("/Cards/Route/resource/1234?x=15&y=test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path2), type);
            Assert.AreEqual("1234", cardRoute.RouteData["resourceId"]);
            Assert.AreEqual("15", cardRoute.QueryData["x"]);
            Assert.AreEqual("test", cardRoute.QueryData["y"]);

            cardRoute = CardRoute.Parse("/Cards/Route/resource/1234/sub/5678?x=15&y=test");
            Assert.IsTrue(rm.ResolveRoute(cardRoute, out type));
            Assert.AreEqual(typeof(Cards_Route_Path3), type);
            Assert.AreEqual("1234", cardRoute.RouteData["value"]);
            Assert.AreEqual("5678", cardRoute.RouteData["subvalue"]);
            Assert.AreEqual("15", cardRoute.QueryData["x"]);
            Assert.AreEqual("test", cardRoute.QueryData["y"]);
        }

    }
}