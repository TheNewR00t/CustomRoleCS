using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SidaLand.CustomRoles
{
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;
        [Description("Whether debug messages should be shown.")]
        public bool Debug { get; set; } = false;
        [Description("Whether Keypress ability activations will work on the server.")]
        public bool UseKeypressActivation { get; set; } = true;
        [Description("Whether abilities that are not selected as the current keypress ability can still be activated.")]
        public bool ActivateOnlySelected { get; set; } = true;
    }
}
