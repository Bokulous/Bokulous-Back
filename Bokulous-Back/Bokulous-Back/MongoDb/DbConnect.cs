using MongoDB.Driver;

namespace Bokulous_Back.MongoDb
{
    public class DbConnect
    {
        MongoClient dbClient;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string Address { get; set; } = "";

        public DbConnect()
        {
            User = "Bokulous";
            Password = File.ReadAllText("./DbPass.txt");
            Address = "@cluster0.vtut1fa.mongodb.net";

            dbClient = new MongoClient("mongodb+srv://" + User + ":" + Password + Address);
        }
        public void Connect()
        {
            var dbList = dbClient.ListDatabases().ToList();

            Console.WriteLine("The list of databases on this server is: ");
            foreach (var db in dbList)
            {
                Console.WriteLine(db);
            }
        }
        
    }
}
