using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Triggered.modules.panel;

namespace Triggered
{
    static class Program
    {
        public static Viewport viewport = new Viewport();
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Program))]
        static async Task Main()
        {
            try
            {
                await viewport.Run();
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
            viewport.Dispose();
        }
    }
}