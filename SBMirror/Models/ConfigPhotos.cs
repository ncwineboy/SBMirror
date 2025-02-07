namespace SBMirror.Models
{
    public class ConfigPhotos : ModuleConfigBase
    {
        public string AlbumName { get; set; } = "Photo Frame";
        public int displayLengthInSeconds { get; set; } = 30;

        public override bool IsValid()
        {
            return (!string.IsNullOrEmpty(AlbumName) && displayLengthInSeconds > 0);
        }
    }
}
