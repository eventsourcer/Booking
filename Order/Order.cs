using System.Security.Cryptography;
using Avro;
using Avro.Specific;

public record Order(
    DateTime OrderTime, 
    int OrderId, 
    int ItemId,
    Status Status, 
    int OrderQuantity,
    int Count,
    Address Address
);
public record Address(string City, string Zipcode);
public enum Status {Pending, Delivered, Rejected};

public record OrderAvroDto(string OrderId, int OrderPrice, string ProductName);
public class OrderAvro(string orderId, int orderPrice, string productName) : ISpecificRecord
{
	public OrderAvro() : this("", 3, "")
	{

	}
    public virtual Schema Schema => Avro.Schema.Parse(@"{""type"":""record"",""name"":""OrderAvro"",""namespace"":""confluent.io.examples.serialization.avro"",""fields"":[{""name"":""orderid"",""type"":""string""},{""name"":""orderprice"",""type"":""int""},{""name"":""productname"",""type"":""string""}]}");
    public string OrderId { get; private set; } = orderId;
    public int OrderPrice { get; private set; } = orderPrice;
    public string ProductName { get; private set; } = productName;
    public virtual object Get(int fieldPos) => (fieldPos) switch
    {
        0 => OrderId,
        1 => OrderPrice,
        2 => ProductName,
        _ => throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()")
    };
    public virtual void Put(int fieldPos, object fieldValue)
	{
		object test = fieldPos switch
		{
			0 => OrderId = (string)fieldValue,
			1 => OrderPrice = (int)fieldValue,
			2 => ProductName = (string)fieldValue,
			_ => throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()")
		};
	}
}
