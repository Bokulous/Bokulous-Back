namespace Bokulous_Back.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public DateTime Date { get; set; } = default(DateTime);
        public List<Book> Books { get; set; } = null!; //kommer bara kunna ha 1 bok/order just nu :(
        public BookUser Buyer { get; set; }
        public string BuyerAdress { get; set; } = "";
        public double BookWeight { get; set; } = 0;
        public double ShippingCost { get; set; } = 0;
        public double TotalBookCost { get; set; } = 0;
        public double TotalOrderCost { get; set; } = 0;

    }
}
