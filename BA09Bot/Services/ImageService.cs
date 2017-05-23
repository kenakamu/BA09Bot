using System.Web;

namespace BA09Bot.Services
{
    public class ImageService : IImageService
    {
        public string GetImageUri(string image)
        {
            var url = HttpContext.Current.Request.Url;
            return $"{url.Scheme}://{url.Host}:{url.Port}/images/{image}";
        }
    }
}