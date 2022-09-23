using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bokulous_Back.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Mail { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public UserBooks[]? Previous_Orders { get; set; }
        public UserBooks[]? Previous_Books_Sold { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSeller { get; set; }
    }
}