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