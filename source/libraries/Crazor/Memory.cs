using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Crazor.Tests")]

namespace Crazor
{
    /// <summary>
    /// Memory is an abstraction over IStorage which segments data 
    /// </summary>
    /// <remarks>
    /// Name => overall segment, for example name of the application
    /// Scope => sub-set of the segment
    /// Essentially each object stored in storage has key composed of
    /// "{name}-{scope}-{key}" 
    /// </remarks>
    public class Memory
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Memory
        /// </summary>
        /// <param name="storage">storage to store memory</param>
        /// <param name="name">name for the memory (We use this for CardApp.Name)</param>
        public Memory(IStorage storage, string name)
        {
            _storage = storage;
            Name = name;
        }

        /// <summary>
        /// Name of the memory
        /// </summary>
        public string Name { get; }

        #region OBJECT
        /// <summary>
        /// Read single object
        /// </summary>
        /// <param name="key">key unique for the object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<object?> GetObjectAsync(string key, CancellationToken cancellationToken = default)
            => GetScopedObjectAsync(String.Empty, key, cancellationToken);


        /// <summary>
        /// Read single object of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">key unique for object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T?> GetObjectAsync<T>(string key, CancellationToken cancellationToken = default)
            => GetScopedObjectAsync<T>(String.Empty, key, cancellationToken);


        /// <summary>
        /// Write a single object
        /// </summary>
        /// <param name="key">key unique for the object</param>
        /// <param name="value">object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SaveObjectAsync(string key, object value, CancellationToken cancellationToken = default)
            => SaveScopedObjectsAsync(String.Empty, new Dictionary<string, object>() { { key, value } }, cancellationToken);

        /// <summary>
        /// Delete a single object
        /// </summary>
        /// <param name="key">key unique for the object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteObjectAsync(string key, CancellationToken cancellationToken = default)
            => DeleteScopedObjectsAsync(String.Empty, [key], cancellationToken);
        #endregion

        #region SCOPED_OBJECT
        /// <summary>
        /// Read single object
        /// </summary>
        /// <param name="scope">scope</param>
        /// <param name="key">key unique for the object in the scope</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<object?> GetScopedObjectAsync(string scope, string key, CancellationToken cancellationToken = default)
        {
            var results = await GetScopedObjectsAsync(scope, [key], cancellationToken);
            return results.FirstOrDefault().Value;
        }

        /// <summary>
        /// Read single object of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope">scope</param>
        /// <param name="key">key unique for the object in the scope</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T?> GetScopedObjectAsync<T>(string scope, string key, CancellationToken cancellationToken = default)
        {
            var results = await GetScopedObjectsAsync<T>(scope, [key], cancellationToken);
            return results.FirstOrDefault().Value;
        }

        /// <summary>
        /// Write a single object
        /// </summary>
        /// <param name="scope">scope</param>
        /// <param name="key">key unique for the object in the scope</param>
        /// <param name="value">object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task GetScopedObjectAsync(string scope, string key, object value, CancellationToken cancellationToken = default)
            => SaveScopedObjectsAsync(scope, new Dictionary<string, object>() { { key, value } }, cancellationToken);

        /// <summary>
        /// Delete  a single objewct
        /// </summary>
        /// <param name="scope">scope</param>
        /// <param name="key">key unique for the object in the scope</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteScopedObjectAsync(string scope, string key, CancellationToken cancellationToken = default)
            => DeleteScopedObjectsAsync(scope, [key], cancellationToken);
        #endregion

        #region OBJECTS
        /// <summary>
        /// Read objects from memory 
        /// </summary>
        /// <param name="keys">keys for objects</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IDictionary<string, object>> GetObjectsAsync(string[] keys, CancellationToken cancellationToken = default)
            => GetScopedObjectsAsync(String.Empty, keys, cancellationToken);

        /// <summary>
        /// Read objects of T from memory 
        /// </summary>
        /// <param name="keys">keys for objects</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IDictionary<string, T>> GetObjectsAsync<T>(string[] keys, CancellationToken cancellationToken = default)
            => GetScopedObjectsAsync<T>(String.Empty, keys, cancellationToken);

        /// <summary>
        /// Write objects to memory 
        /// </summary>
        /// <param name="changes">unique keys for objects and their values</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SaveObjectsAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
            => SaveScopedObjectsAsync(String.Empty, changes, cancellationToken);

        /// <summary>
        /// Delete objects in memory 
        /// </summary>
        /// <param name="keys">unique keys for objects to delete</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteObjectsAsync(string[] keys, CancellationToken cancellationToken = default)
            => DeleteScopedObjectsAsync(String.Empty, keys, cancellationToken);
        #endregion

        #region SCOPED_OBJECTS
        /// <summary>
        /// Read objects from memory scope
        /// </summary>
        /// <param name="scope">the scope to segment the data to</param>
        /// <param name="keys">keys for objects unique to the scope </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, object>> GetScopedObjectsAsync(string scope, string[] keys, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scope);
            var innerKeys = keys.Select(key => GetScopedKey(scope, key)!).ToArray();
            var results = await _storage.ReadAsync(innerKeys, cancellationToken);
            var mappedResults = new Dictionary<string, object>();
            foreach (var key in keys)
            {
                if (results.TryGetValue(GetScopedKey(scope, key), out var val))
                    mappedResults[key] = val;
            }
            return mappedResults;
        }

        /// <summary>
        /// Read objects from memory scope
        /// </summary>
        /// <param name="scope">scope</param>
        /// <param name="keys">keys for each object unique to the scope</param>
        /// <param name="cancellationToken"></param>
        public async Task<IDictionary<string, T>> GetScopedObjectsAsync<T>(string scope, string[] keys, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scope);
            var innerKeys = keys.Select(key => GetScopedKey(scope, key)!).ToArray();
            var results = await _storage.ReadAsync(innerKeys, cancellationToken);
            var mappedResults = new Dictionary<string, T>();
            foreach (var key in keys)
            {
                if (results.TryGetValue(GetScopedKey(scope, key), out var val) && val != null)
                    mappedResults[key] = JObject.FromObject(val).ToObject<T>()!;
            }
            return mappedResults;
        }

        /// <summary>
        /// Write objects to memory scope
        /// </summary>
        /// <param name="scope">the scope to segment the data to</param>
        /// <param name="changes">map of keys to values</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SaveScopedObjectsAsync(string scope, IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scope);
            var scopedChanges = new Dictionary<string, object>();
            foreach (var change in changes)
            {
                scopedChanges[GetScopedKey(scope, change.Key)!] = change.Value;
            }
            return _storage.WriteAsync(scopedChanges, cancellationToken);
        }

        /// <summary>
        /// Delete objects in memory scope
        /// </summary>
        /// <param name="scope">the scope to segment the data to</param>
        /// <param name="keys">keys delete in the scope</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteScopedObjectsAsync(string scope, string[] keys, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scope);
            var scopedKeys = keys.Select(key => GetScopedKey(scope, key)).ToArray();
            return _storage.DeleteAsync(scopedKeys, cancellationToken);
        }
        #endregion

        internal string? GetObjectKey(string? key)
            => GetScopedKey(String.Empty, key);

        internal string? GetScopedKey(string scope, string? key)
        {
            ArgumentNullException.ThrowIfNull(scope);
            if (String.IsNullOrEmpty(key))
                return null;
            return $"{this.Name}-{scope}-{key ?? "data"}";
        }

        //internal string ReverseKey(string scope, string key)
        //{
        //    ArgumentNullException.ThrowIfNull(scope);

        //    var prefix = $"{this.Name}-{scope}-";
        //    if (key.StartsWith(prefix))
        //        return key.Substring(prefix.Length);
        //    return key;
        //}

    }
}
