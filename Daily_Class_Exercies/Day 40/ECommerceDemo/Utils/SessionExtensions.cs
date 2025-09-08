using System.Text.Json;

namespace ECommerceDemo.Utils
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            return str == null ? default : JsonSerializer.Deserialize<T>(str);
        }
    }
}
