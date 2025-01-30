using System.Dynamic;

namespace SBMirror.Models
{
    public class Config
    {
        public string units { get; set; } = "imperial";

        public List<Module> modules { get; set; } = new List<Module>();
    }

    public class Module
    {
        public string name { get; set; } = "imperial";
        public string position { get; set; } = "middle center";
        public string? header { get; set; }

        public dynamic config { get; set; } = new ExpandoObject();
    }
}
