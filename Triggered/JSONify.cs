using System.Windows.Forms;
using Newtonsoft.Json;


namespace Triggered
{
    public partial class App_ParentFrame2 : Form
    {

        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        private string JSONify(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, settings);
            return json;
        }

    }
}