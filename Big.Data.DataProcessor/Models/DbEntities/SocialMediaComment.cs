using MongoDB.Bson.Serialization.Attributes;

namespace Big.Data.DataProcessor.Models.DbEntities;

public class SocialMediaComment
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("date")]
    public required string Date { get; set; }

    [BsonElement("comment")]
    public required string Comment { get; set; }

    public SocialMediaComment() { }
}
