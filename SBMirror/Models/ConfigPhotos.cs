namespace SBMirror.Models
{
    public class ConfigPhotos : ModuleConfigBase
    {
        public string clientJson { get; set; } = string.Empty;
        public string albumName { get; set; } = string.Empty;
        public int displayLengthInSeconds { get; set; } = 30;

        public override bool IsValid()
        {
            return (!string.IsNullOrEmpty(clientJson) && !string.IsNullOrEmpty(albumName) && displayLengthInSeconds > 0);
        }
    }
}
