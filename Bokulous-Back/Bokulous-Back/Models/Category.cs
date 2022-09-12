using Bokulous_Back.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bokulous_Back.Models
{
    public class Category : IItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        //TODO: Får vara null?
        public string? Id { get; set; }
        public string Name { get; set; }
    }
}