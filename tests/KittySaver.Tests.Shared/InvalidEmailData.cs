namespace KittySaver.Tests.Shared;

public class InvalidEmailData : TheoryData<string>
{
        public InvalidEmailData()
        {
            
            Add("plainaddress");
            Add("@missingusername.com");
            Add("missingatsign.com");
            Add("username@.com");
            Add("username@com");
            Add("username@missingtld.");
            Add("username@.missingtld");
            Add("username@domain,com");
            Add("username@domain#com");
            Add("username@domain!com");
            Add("username@domain.com (Joe Smith)");
            Add("username@domain.com>");
            Add("user name@domain.com");
            Add("username@ domain.com");
            Add("username@domain .com");
            Add(" username@domain.com");
            Add("username@domain.com ");
        }
}