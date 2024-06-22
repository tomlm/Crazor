using Microsoft.Bot.Builder;

namespace Crazor.Tests
{
    [TestClass]
    public class MemoryStorageTests
    {

        [TestMethod]
        public void TestScopedKey()
        {
            var storage = new MemoryStorage();
            Memory memory = new Memory(storage, "app");
            var scopedKey = memory.GetScopedKey(String.Empty, "key");
            Assert.AreEqual("app//key", scopedKey);

            scopedKey = memory.GetScopedKey("test", "key");
            Assert.AreEqual("app/test/key", scopedKey);
            //var unscopedKey = memory.ReverseKey("test", scopedKey!);
            //Assert.AreEqual("key", unscopedKey);

            memory = new Memory(storage, "app2");
            scopedKey = memory.GetScopedKey("test2", "key2");
            Assert.AreEqual("app2/test2/key2", scopedKey);
            //unscopedKey = memory.ReverseKey("test2", scopedKey!);
            //Assert.AreEqual("key2", unscopedKey);
        }

        [TestMethod]
        public async Task TestScopedObjectsMemory()
        {
            var storage = new MemoryStorage();
            Memory memory = new Memory(storage, "app");

            var results = await memory.GetScopedObjectsAsync("test", ["unknown"]);
            Assert.AreEqual(0, results.Count);

            string key = "x";
            Dictionary<string, object> data = new Dictionary<string, object>() { { key, new PocoItem() { Name = "foo" } } };

            await memory.SaveScopedObjectsAsync("test", data);

            string rawKey = memory.GetScopedKey("test", key)!;
            var rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(1, rawResult.Count);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(rawResult[rawKey], "Name"));

            results = await memory.GetScopedObjectsAsync("test", [key]);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(results[key], "name"));

            var resultsT = await memory.GetScopedObjectsAsync<PocoItem>("test", [key]);
            Assert.AreEqual(1, resultsT.Count);
            Assert.IsNotNull(resultsT[key]);
            Assert.AreEqual("foo", resultsT[key].Name);

            await memory.DeleteScopedObjectsAsync("test", [key]);

            rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(0, rawResult.Count);

            results = await memory.GetScopedObjectsAsync("test", [key]);
            Assert.AreEqual(0, results.Count);

            resultsT = await memory.GetScopedObjectsAsync<PocoItem>("test", [key]);
            Assert.AreEqual(0, resultsT.Count);
        }

        [TestMethod]
        public async Task TestObjectsMemory()
        {
            var storage = new MemoryStorage();
            Memory memory = new Memory(storage, "app");

            var results = await memory.GetObjectsAsync(new[] { "unknown" });
            Assert.AreEqual(0, results.Count);

            string key = "x";
            Dictionary<string, object> data = new Dictionary<string, object>() { { key, new { name = "foo" } } };

            await memory.SaveObjectsAsync(data);

            string rawKey = memory.GetObjectKey(key)!;
            var rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(1, rawResult.Count);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(rawResult[rawKey], "name"));

            results = await memory.GetObjectsAsync([key]);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(results[key], "name"));

            var resultsT = await memory.GetObjectsAsync<PocoItem>([key]);
            Assert.AreEqual(1, resultsT.Count);
            Assert.AreEqual("foo", resultsT[key].Name);

            await memory.DeleteObjectsAsync([key]);

            rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(0, rawResult.Count);

            results = await memory.GetObjectsAsync([key]);
            Assert.AreEqual(0, results.Count);

            resultsT = await memory.GetObjectsAsync<PocoItem>([key]);
            Assert.AreEqual(0, resultsT.Count);
        }

        [TestMethod]
        public async Task TestObjectMemory()
        {
            var storage = new MemoryStorage();
            Memory memory = new Memory(storage, "app");

            var result = await memory.GetObjectAsync("unknown");
            Assert.IsNull(result);

            string key = "x";
            Dictionary<string, object> data = new Dictionary<string, object>() { { key, new { name = "foo" } } };

            await memory.SaveObjectAsync("x", new { name = "foo" });

            string rawKey = memory.GetObjectKey(key)!;
            var rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(1, rawResult.Count);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(rawResult[rawKey], "name"));

            result = await memory.GetObjectAsync(key);
            Assert.IsNotNull(result);
            Assert.AreEqual("foo", ObjectPath.GetPathValue<string>(result, "name"));

            var resultT = await memory.GetObjectAsync<PocoItem>(key);
            Assert.AreEqual("foo", resultT?.Name);

            await memory.DeleteObjectAsync(key);

            rawResult = await storage.ReadAsync([rawKey], default);
            Assert.AreEqual(0, rawResult.Count);

            result = await memory.GetObjectAsync(key);
            Assert.IsNull(result);

            resultT = await memory.GetObjectAsync<PocoItem>(key);
            Assert.IsNull(resultT);
        }

        internal class PocoItem
        {
            public string Name { get; set; }
        }
    }
}