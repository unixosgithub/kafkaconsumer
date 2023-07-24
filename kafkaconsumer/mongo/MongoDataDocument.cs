using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace kafkaconsumer.mongo
{
    [BsonIgnoreExtraElements]
    public class MongoDataDocument
    {
        [JsonIgnore]
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("value")]
        public string Value { get; set; }
    }
}
