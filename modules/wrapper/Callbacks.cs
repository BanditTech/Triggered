namespace Triggered.modules.wrapper
{
    public static class Callbacks
    {
        public static void FontIndexEdit(int index)
        {
            if (App.Options == null || App.logger == null || App.fonts == null)
                return;

            App.Log("Callback successfully fired");
            if (App.Options.Font.GetKey<string>("Name") != App.fonts[index])
                App.Options.Font.SetKey("Name", App.fonts[index]);
        }
    }
}
