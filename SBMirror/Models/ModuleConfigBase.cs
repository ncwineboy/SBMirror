namespace SBMirror.Models
{
    public abstract class ModuleConfigBase
    {
        public int intervalInSeconds { get; set; }

        public abstract bool IsValid();
    }
}
