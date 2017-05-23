namespace BA09Bot.Services
{
    public interface IAzureBlobService
    {
        string Upload(string base64Image, string name);
    }
}