namespace KittySaver.Api.Shared.Domain.Common.Interfaces;

public interface IContact
{
    public string Email { get; }
    public string PhoneNumber { get; }
    
    public static class Constraints
    {
        public const int EmailMaxLength = 254;
        public const int PhoneNumberMaxLength = 31;
        public const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    }
}