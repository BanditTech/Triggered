namespace Triggered
{
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main()
        {
            using var overlay = new TriggeredMenu();
            await overlay.Run();
        }
    }
}