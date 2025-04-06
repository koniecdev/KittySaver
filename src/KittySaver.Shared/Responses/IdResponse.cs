namespace KittySaver.Shared.Responses;

public record IdResponse(Guid Id);
public record IdResponse<TId>(TId Id) where TId : struct
{
    public override string ToString()
    {
        return Id.ToString()!;
    }
    
    public static implicit operator TId(IdResponse<TId> response)
    {
        return response.Id;
    }
}
