using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Theme_Implemenet.Models
{
    public class ShoppingCarts
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ProductVeriation")]
        public int veriationId { get; set; }
        public virtual ProductVeriation ProductVeriation { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
        public string color { get; set; }
        public int Price { get; set; }
        public decimal bill { get; set; }
        public string image { get; set; }

    }

    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

}

