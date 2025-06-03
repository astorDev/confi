namespace Confi.Manager;

public partial class Uris
{
    public static string Configuration = "configuration";
    public static string AppConfiguration(string appId) => $"{App(appId)}/{Configuration}";
}