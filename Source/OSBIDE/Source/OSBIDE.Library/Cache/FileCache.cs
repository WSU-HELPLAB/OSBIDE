using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace OSBIDE.Library.Cache
{
    public class FileCache : ObjectCache
    {
        private static int _nameCounter = 1;
        private string _name = "";
        private static List<FileCache> _instances = new List<FileCache>();
        
        public string CacheDir { get; private set; }
        private static string DefaultCachePath
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }

        private FileCache()
        {
            _name = "FileCache_" + _nameCounter;
            _nameCounter++;
            CacheDir = DefaultCachePath;
        }

        public FileCache(string cacheRoot) : this()
        {
            CacheDir = cacheRoot;
        }

        public static FileCache GetInstance(string cachePath)
        {
            FileCache instance = _instances.Where(fc => fc.CacheDir.ToLower().CompareTo(cachePath.ToLower()) == 0)
                                           .FirstOrDefault();
            if (instance == null)
            {
                instance = new FileCache(cachePath);
                _instances.Add(instance);
            }
            return instance;
        }

        public static FileCache GetInstance()
        {
            return GetInstance(DefaultCachePath);
        }

        private string GetPath(string key, string regionName = null)
        {
            string directory = Path.Combine(CacheDir, regionName);
            string filePath = Path.Combine(directory, key + ".dat");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return filePath;
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            string path = GetPath(key, regionName);
            object oldData = null;

            //pull old value if it exists
            if (File.Exists(path))
            {
                oldData = Get(key, regionName);
            }
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);
            }

            //As documented in the spec (http://msdn.microsoft.com/en-us/library/dd780602.aspx), return the old
            //cahced value or null
            return oldData;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            object oldData = AddOrGetExisting(value.Key, value.Value, policy);
            CacheItem returnItem = null;
            if (oldData != null)
            {
                returnItem = new CacheItem(value.Key)
                {
                    Value = oldData
                };
            }
            return returnItem;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return AddOrGetExisting(key, value, new CacheItemPolicy(), regionName);
        }

        public override bool Contains(string key, string regionName = null)
        {
            string path = GetPath(key, regionName);
            return File.Exists(path);
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                //AC note: can use boolean OR "|" to set multiple flags.
                return System.Runtime.Caching.DefaultCacheCapabilities.CacheRegions;
            }
        }

        public override object Get(string key, string regionName = null)
        {
            string path = GetPath(key, regionName);
            object data = null;
            if (File.Exists(path))
            {
                FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
                BinaryFormatter formatter = new BinaryFormatter();

                //AC: From http://spazzarama.com//2009/06/25/binary-deserialize-unable-to-find-assembly/
                //    There was a problem with deserilization of my custom objects.  The above link
                //    appears to have fixed the problem.
                formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                data = formatter.Deserialize(stream);
                stream.Close();
            }
            return data;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            CacheItem item = (CacheItem)Get(key, regionName);
            return item;
        }

        public override long GetCount(string regionName = null)
        {
            string path = GetPath("", regionName);
            return Directory.GetFiles(path).Count();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return _name; }
        }

        public override object Remove(string key, string regionName = null)
        {
            object valueToDelete = null;
            if (Contains(key))
            {
                valueToDelete = Get(key, regionName);
                string path = GetPath(key, regionName);
                File.Delete(path);
            }
            return valueToDelete;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            Add(key, value, policy, regionName);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Add(item, policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Add(key, value, absoluteExpiration, regionName);
        }

        public override object this[string key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                this.Set(key, value, policy);
            }
        }

        sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;

                String currentAssembly = Assembly.GetExecutingAssembly().FullName;

                // In this case we are always using the current assembly
                assemblyName = currentAssembly;

                // Get the type using the typeName and assemblyName
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));

                return typeToDeserialize;
            }
        }
    }
}
