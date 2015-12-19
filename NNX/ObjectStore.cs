using System.Collections.Generic;

namespace NNX
{
    public static class ObjectStore
    {
        private static readonly Dictionary<string, object> Objects =
            new Dictionary<string, object>(); 

        public static void Add(string name, object obj)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new NNXException("Object name cannot be null or empty.");
            }

            Objects[name] = obj;
        }

        public static T Get<T>(string name)
        {
            object obj;

            if (!Objects.TryGetValue(name, out obj))
                throw new NNXException($"No such object: '{name}'");

            if (obj is T)
                return (T) obj;

            var expectedType = typeof (T).Name;
            var actualType = obj.GetType().Name;

            throw new NNXException($"Object '{name}' was expected to be {expectedType}" +
                $" but was {actualType}.");
        }

        public static void Clear()
        {
            Objects.Clear();
        }
    }
}
