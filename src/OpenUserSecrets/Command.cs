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
            ThreadHelper.ThrowIfNotOnUIThread();
            var root = XElement.Load(path);
            var ns = root.Name.Namespace;
            var element = root.Elements(ns + "PropertyGroup").Elements(ns + key).FirstOrDefault();
            if (element == null)
            {
                root.Element(ns + "PropertyGroup").Add(new XElement(ns + key, guid));
                using (var stream = new StreamWriter(path, false, new System.Text.UTF8Encoding(false)))
                {
                    // TODO : remove unnecessary namespace
                    // TODO : keep original empty lines.
                    root.Save(stream);
                }
            }
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
