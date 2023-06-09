namespace Triggered.modules.wrapper
{
    internal static class Callbacks
    {
        internal static void FontIndexEdit(int index)
        {
            App.Options.Font.SetKey("Name", App.fonts[index]);
        }
    }
}
