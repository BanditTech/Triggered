namespace Triggered.modules.wrapper
{
    internal static class Callbacks
    {
        internal static void FontIndexEdit(int index)
        {
            App.Log("Callback fired");
            App.Options.Font.SetKey("Name", App.fonts[index]);
        }
    }
}
