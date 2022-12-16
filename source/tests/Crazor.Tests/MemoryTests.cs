using Crazor;
using Crazor.Test;
using Crazor.Tests.Cards.Memory;

namespace Crazor.Tests
{
    [TestClass]
    public class MemoryTests : CardTest
    {
        [TestInitialize]
        public async Task TestInitialize()
        {
            var cardApp = (MemoryApp)Factory.Create(CardRoute.Parse("/Cards/Memory"));
            await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default);
            cardApp.App = "App1";
            cardApp.Session = "Session1";
            cardApp.User = "User1";
            cardApp.Conversation = "Conversation1";
            cardApp.Path = "Path1";
            cardApp.Temp = "Temp1";
            await cardApp.SaveAppAsync(default(CancellationToken));
        }

        [TestMethod]
        public async Task TestMemory()
        {
            var cardRoute = CardRoute.Parse("/Cards/Memory");
            // validate load
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default);

                Assert.AreEqual("App1", cardApp.App);
                Assert.AreEqual("Session1", cardApp.Session);
                Assert.AreEqual("User1", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
            }

            // validate App Save
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default);

                Assert.AreEqual("App1", cardApp.App);
                Assert.AreEqual("Session1", cardApp.Session);
                Assert.AreEqual("User1", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
                Assert.IsNull(cardApp.Temp);

                cardApp.Temp = "Test1";
                cardApp.App = "App2";
                await cardApp.SaveAppAsync(default(CancellationToken));
            }

            // validate session
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default);

                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session1", cardApp.Session);
                Assert.AreEqual("User1", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
                Assert.IsNull(cardApp.Temp);

                cardApp.Temp = "Test1";
                cardApp.Session = "Session2";
                await cardApp.SaveAppAsync(default(CancellationToken));
            }

            // validate user
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default);

                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session2", cardApp.Session);
                Assert.AreEqual("User1", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
                Assert.IsNull(cardApp.Temp);

                cardApp.Temp = "Test1";
                cardApp.User = "User2";
                await cardApp.SaveAppAsync(default(CancellationToken));
            }

            // validate Conversation
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default(CancellationToken));
                Assert.IsNull(cardApp.Temp);

                cardApp.Temp = "Test1";
                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session2", cardApp.Session);
                Assert.AreEqual("User2", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);

                cardApp.Conversation = "Conversation2";
                await cardApp.SaveAppAsync(default(CancellationToken));
            }

            // validate Path
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default(CancellationToken));

                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session2", cardApp.Session);
                Assert.AreEqual("User2", cardApp.User);
                Assert.AreEqual("Conversation2", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
                Assert.IsNull(cardApp.Temp);

                cardApp.Temp = "Test1";
                cardApp.Path = "Path2";
                await cardApp.SaveAppAsync(default(CancellationToken));
            }

            // validate Path
            {
                var cardApp = (MemoryApp)Factory.Create(cardRoute);
                await cardApp.LoadAppAsync(CreateInvokeActivity().CreateLoadRouteActivity(cardApp.Route.Route), default(CancellationToken));

                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session2", cardApp.Session);
                Assert.AreEqual("User2", cardApp.User);
                Assert.AreEqual("Conversation2", cardApp.Conversation);
                Assert.AreEqual("Path2", cardApp.Path);
            }
        }
    }
}