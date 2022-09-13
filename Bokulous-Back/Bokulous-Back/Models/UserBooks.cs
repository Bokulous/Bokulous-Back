using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bokulous_Back.Models
{
    public class UserBooks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ISBN { get; set; } = "";
        public string Title { get; set; } = "";
        public string[] Categories { get; set; } = null!;
        public string Language { get; set; } = "";
        public string[] Authors { get; set; } = null!;
        public int Published { get; set; }
        public int Weight { get; set; }
        public bool IsUsed { get; set; }
        public int InStorage { get; set; }
        public double Price { get; set; }
    }
}