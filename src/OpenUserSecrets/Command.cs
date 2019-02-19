using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenUserSecrets
{
    internal interface ICommand
    {
        void Execute();
    }

    internal class UserSecretFileOpenableCommand : ICommand
    {
        private readonly EnvDTE.DTE dte;
        private readonly string path;
        public UserSecretFileOpenableCommand(EnvDTE.DTE dte, string path)
        {
            this.dte = dte;
            this.path = path;
        }

        public void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            dte.ExecuteCommand("File.OpenFile", path);
        }
    }

    internal class UserSecretFileNotExistsCommand : ICommand
    {
        private readonly string path;

        public UserSecretFileNotExistsCommand(string path)
        {
            this.path = path;
        }

        public void Execute()
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(path))
            {
                using (File.Create(path)) { }
            }
        }
    }

    internal class UserSecretsEntryNotExistsCommand : ICommand
    {
        private static readonly int baseSpaceNum = 2;
        private static readonly bool containsBom = false;

        private readonly string key;
        private readonly string path;
        private readonly string guid;

        public UserSecretsEntryNotExistsCommand(string key, string path)
        {
            this.key = key;
            this.path = path;
            this.guid = Guid.NewGuid().ToString();
        }

        public void Execute()
        {
            var root = XElement.Load(path, LoadOptions.PreserveWhitespace);
            var ns = root.Name.Namespace;
            var element = root.Elements(ns + "PropertyGroup").Elements(ns + key).FirstOrDefault();
            if (element == null)
            {
                // get space
                var elements = root.Element(ns + "PropertyGroup").Elements().Select(x => x?.ToString()).Where(x => x != null).ToArray();
                var space = GetIntentSpace("<PropertyGroup>", elements);

                // insert element
                root.Element(ns + "PropertyGroup").Add(space, new XElement(ns + key, guid), "\n", space);
                var xml = root.ToString();
                
                // add line end
                xml += '\n';
                
                // write
                var bytes = new System.Text.UTF8Encoding(containsBom).GetBytes(xml);
                File.WriteAllBytes(path, bytes);
            }
        }

        private string GetIntentSpace(string element, string[] insideElement)
        {
            var file = File.ReadAllLines(path);
            var elementSpace = file.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement.SelectMany(y => file.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min();
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }
    }

    internal class MissingPackageCommand : ICommand
    {
        private readonly AsyncPackage package;
        private readonly string title;
        private readonly string message;
        public MissingPackageCommand(AsyncPackage package, string title, string message)
        {
            this.package = package;
            this.title = title;
            this.message = message;
        }

        public void Execute()
        {
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
