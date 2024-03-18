namespace NavigatingStars
{
    /// <summary>
    /// The PluginInfo class contains constant values that provide information about the plugin.
    /// These values are used throughout the plugin's code and can be accessed by other classes.
    /// </summary>
    public static class PluginInfo
    {
        /// <summary>
        /// The unique identifier for the plugin.
        /// This value should be a string in the format "com.author.pluginname".
        /// </summary>
        public const string PLUGIN_GUID = "com.nilaier.navigatingstars";

        /// <summary>
        /// The display name of the plugin.
        /// This value is used in user-facing elements, such as the plugin manager or UI.
        /// </summary>
        public const string PLUGIN_NAME = "Navigating Stars";

        /// <summary>
        /// The current version of the plugin.
        /// This value should be updated whenever a new version of the plugin is released.
        /// It is recommended to follow semantic versioning (e.g., "1.0.0", "1.2.3").
        /// </summary>
        public const string PLUGIN_VERSION = "0.0.3";
    }
}