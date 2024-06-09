namespace ChatApplication.Helpers
{
    public class FileHelper
    {
        public static string GetFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }

            return Path.GetExtension(filePath).ToLower();
        }
        public static string GetFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }

            return Path.GetFileName(filePath);
        }
    }
}
