using Crazor.Test;
using Crazor.Tests.Cards.Memory;
using Microsoft.Bot.Builder;

namespace Crazor.Tests
{
    [TestClass]
    public class MemoryTests : CardTest
    {
        [TestInitialize]
        public async Task TestInitialize()
        {
            var cardRoute = CardRoute.Parse("/Cards/Memory/test");
            var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;
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
            var cardRoute = CardRoute.Parse("/Cards/Memory/test");
            // validate load
            {
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

                Assert.AreEqual("App1", cardApp.App);
                Assert.AreEqual("Session1", cardApp.Session);
                Assert.AreEqual("User1", cardApp.User);
                Assert.AreEqual("Conversation1", cardApp.Conversation);
                Assert.AreEqual("Path1", cardApp.Path);
            }

            // validate App Save
            {
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

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
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

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
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

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
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;
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
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

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
                var cardApp = (MemoryApp)(await LoadCard(cardRoute)).App;

                Assert.AreEqual("App2", cardApp.App);
                Assert.AreEqual("Session2", cardApp.Session);
                Assert.AreEqual("User2", cardApp.User);
                Assert.AreEqual("Conversation2", cardApp.Conversation);
                Assert.AreEqual("Path2", cardApp.Path);
            }
        }
    }
}