using System.Configuration;

namespace Berichtsheft_Editor_X_API
{
    public class CustomSettings
    {
        public string PrivateKeyLocation { get; set; }
        public string UploadFolderPath { get; set; }

        public CustomSettings(IConfiguration config)
        {
            PrivateKeyLocation = config.GetSection("CustomSettings").GetSection("PrivateKeyLocation").Value;
            UploadFolderPath = config.GetSection("CustomSettings").GetSection("UploadFolderPath").Value;
        }
    }
}