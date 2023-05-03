namespace Triggered
{
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main()
        {
            using var overlay = new TriggeredMenu();
            try
            {
                await overlay.Run();
            }
            finally
            {
                OnProgramExit();
            }
        }
        static void OnProgramExit()
        {
            // Begin to release resources
            App.Log("Final execution block initiated. Releasing memory resources.");
        }
    }
}