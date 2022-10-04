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
        public bool IsActive { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public List<UserBooks>? Previous_Orders { get; set; }
        public List<UserBooks>? Previous_Books_Sold { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsSeller { get; set; }
        public string ActivationCode { get; set; } = "";
    }
}