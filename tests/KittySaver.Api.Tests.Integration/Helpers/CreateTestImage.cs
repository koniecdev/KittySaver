namespace KittySaver.Api.Tests.Integration.Helpers;

public static class CreateTestImageHelper
{
    public static MemoryStream Create(int width = 100, int height = 100)
    {
        MemoryStream stream = new();
        byte[] bytes = new byte[width * height * 3]; // RGB data
        new Random().NextBytes(bytes);
        stream.Write(bytes, 0, bytes.Length);
        stream.Position = 0;
        return stream;
    }
}