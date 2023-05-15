namespace Triggered
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Triggered.modules.panels;
    using ImNodesNET;

    class Program
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