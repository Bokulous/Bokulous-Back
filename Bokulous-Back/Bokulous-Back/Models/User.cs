using Bokulous_Back.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bokulous_Back.Models
{
    public class User : IItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Mail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool isActive { get; set; }
        public bool isBlocked { get; set; }
        public UserBooks[] Previous_Orders { get; set; }
        public bool isAdmin { get; set; }
        public bool isSeller { get; set; }
    }
}
