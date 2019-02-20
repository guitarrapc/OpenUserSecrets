using EnvDTE;
using EnvDTE80;
using System;

namespace OpenUserSecrets
{
    public static class Output
    {
        private static readonly string paneName = nameof(OpenUserSecrets);
        private const string openMessage = @"
__      ___                 _  _____ _             _ _       ______      _             _   _
\ \    / (_)               | |/ ____| |           | (_)     |  ____|    | |           | | (_)
 \ \  / / _ ___ _   _  __ _| | (___ | |_ _   _  __| |_  ___ | |__  __  _| |_ ___ _ __ | |_ _  ___  _ __
  \ \/ / | / __| | | |/ _` | |\___ \| __| | | |/ _` | |/ _ \|  __| \ \/ / __/ _ \ '_ \| __| |/ _ \| '_ \
   \  /  | \__ \ |_| | (_| | |____) | |_| |_| | (_| | | (_) | |____ >  <| ||  __/ | | | |_| | (_) | | | |
    \/   |_|___/\__,_|\__,_|_|_____/ \__|\__,_|\__,_|_|\___/|______/_/\_\\__\___|_| |_|\__|_|\___/|_| |_|
  ____                   _    _               _____                    _
 / __ \                 | |  | |             / ____|                  | |
| |  | |_ __   ___ _ __ | |  | |___  ___ _ _| (___   ___  ___ _ __ ___| |_ ___
| |  | | '_ \ / _ \ '_ \| |  | / __|/ _ \ '__\___ \ / _ \/ __| '__/ _ \ __/ __|
| |__| | |_) |  __/ | | | |__| \__ \  __/ |  ____) |  __/ (__| | |  __/ |_\__ \
 \____/| .__/ \___|_| |_|\____/|___/\___|_| |_____/ \___|\___|_|  \___|\__|___/
       | |
       |_|
";
        public static void PrintMessageLine(this EnvDTE.DTE dte, string message)
        {
            dte.PrintMessage(message + "\n");
        }

        public static void PrintMessage(this EnvDTE.DTE dte, string message)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var panes = ((DTE2)dte).ToolWindows.OutputWindow.OutputWindowPanes;
            OutputWindowPane outputPane = null;
            try
            {
                outputPane = panes.Item(paneName);
            }
            catch (ArgumentException)
            {
                panes.Add(paneName);
                outputPane = panes.Item(paneName);
                outputPane.OutputString(openMessage);
            }

            outputPane.OutputString(message);
        }
    }
}
