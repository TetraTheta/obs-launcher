using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OBSLauncher {
  internal static class Program {
    [STAThread]
    static void Main() {
      // Load embedded DLL (https://stackoverflow.com/a/62929101)
      AppDomain.CurrentDomain.AssemblyResolve += FatExecutable.OnResolveAssembly;

      string obsStudioPath = "bin/64bit/obs64.exe";

      // Check obs64.exe exists
      bool obsExists = File.Exists(obsStudioPath);

      if (!obsExists) {
        // OBS Studio is not found - Download it
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Window());
      }

      if (!File.Exists(obsStudioPath)) {
        throw new Exception("OBS Studio executable is not found!");
      }

      string [] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
      // Open OBS Studio
      Process obsStudio = new Process();
      obsStudio.StartInfo.FileName = Path.GetFullPath(obsStudioPath);
      obsStudio.StartInfo.Arguments = string.Join(" ", args);
      obsStudio.StartInfo.UseShellExecute = false;
      obsStudio.StartInfo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, obsStudioPath));
      obsStudio.Start();

      // Exit OBS Launcher
      Environment.Exit(0);
    }
  }
}
