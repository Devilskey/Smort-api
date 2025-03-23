namespace Tiktok_api.Settings_Api
{
    public static class ContentSizingObjects
    {
        public static ContentSize[] Thumbnails = new ContentSize[]
        {
            new ContentSize() { Width = 500, Size = Sizes.L },
            new ContentSize() { Width = 250, Size = Sizes.M },
            new ContentSize() { Width = 150, Size = Sizes.S }
        };

        public static ContentSize[] Content = new ContentSize[]
        {
            new ContentSize() { Width = 1080, Size = Sizes.L },
            new ContentSize() { Width = 720, Size = Sizes.M },
            new ContentSize() { Width = 480, Size = Sizes.S }
        }; 
        public static ContentSize[] ProfilePictures = new ContentSize[]
        {
            new ContentSize() { Width = 250, Size = Sizes.L },
            new ContentSize() { Width = 100, Size = Sizes.M },
            new ContentSize() { Width = 50, Size = Sizes.S }
        };

    }

    public class ContentSize
    {
        public int Width { get; set; }
        public Sizes Size { get; set; }
    }

    public enum Sizes
    {
        XS,
        S,
        M,
        L,
        XL,
    }
}
