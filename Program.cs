namespace Triggered
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
using Triggered.modules.panel;

    static class Program
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Program))]
        static async Task Main()
        {
            using var overlay = new MainMenu();
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