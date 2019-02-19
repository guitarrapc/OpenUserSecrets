using Microsoft.Build.Evaluation;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenUserSecrets
{
    internal class UserSecretStateMachine
    {
        internal enum State
        {
            MissingPackage = 0,
            UserSecretsEntryNotExists,
            UserSecretFileNotExists,
            UserSecretFileOpenable,
        }

        private static readonly string fileName = "secrets.json";

        public ICommand Command { get; private set; }
        public State Current { get; private set; } = State.MissingPackage;

        private readonly ProjectCollection collection;
        private readonly string path;
        private readonly string propertyKey;
        private readonly EnvDTE.DTE dte;
        private readonly Microsoft.VisualStudio.Shell.AsyncPackage package;
        private string userSecretId;

        public UserSecretStateMachine(string path, string propertyKey, EnvDTE.DTE dte, Microsoft.VisualStudio.Shell.AsyncPackage package)
        {
            this.collection = new Microsoft.Build.Evaluation.ProjectCollection();
            this.path = path;
            this.propertyKey = propertyKey;
            this.dte = dte;
            this.package = package;

            MoveNext();
        }

        public bool MoveNext()
        {
            UpdateState();
            Command = UpdateCommand();
            switch (Current)
            {
                case State.MissingPackage:
                    return false;
                case State.UserSecretsEntryNotExists:
                    return true;
                case State.UserSecretFileNotExists:
                    return true;
                case State.UserSecretFileOpenable:
                    return false;
            }
            return false;
        }

        private void UpdateState()
        {
            collection.UnloadAllProjects();
            collection.LoadProject(path);

            var csproj = collection.GetLoadedProjects(path).FirstOrDefault();
            var isPackageExists = csproj.Items
                .Where(x => x.ItemType == "PackageReference")
                .Where(x => x.EvaluatedInclude == "Microsoft.Extensions.Configuration.UserSecrets")
                .Any();
            this.userSecretId = csproj.GetPropertyValue(propertyKey);

            if (!isPackageExists)
            {
                Current = State.MissingPackage;
            }
            else if (string.IsNullOrWhiteSpace(this.userSecretId))
            {
                Current = State.UserSecretsEntryNotExists;
            }
            else if (!File.Exists(GetUserSecretFilePath(userSecretId)))
            {
                Current = State.UserSecretFileNotExists;
            }
            else
            {
                Current = State.UserSecretFileOpenable;
            }
        }

        private ICommand UpdateCommand()
        {
            ICommand command = null;
            switch (Current)
            {
                case State.MissingPackage:
                    var title = "Manage UserSecrets";
                    var message = "Please install required pacakge Microsoft.Extensions.Configuration.UserSecrets";
                    command = new MissingPackageCommand(package, title, message);
                    break;
                case State.UserSecretsEntryNotExists:
                    command = new UserSecretsEntryNotExistsCommand(propertyKey, path);
                    break;
                case State.UserSecretFileNotExists:
                    command = new UserSecretFileNotExistsCommand(GetFilePath());
                    break;
                case State.UserSecretFileOpenable:
                    command = new UserSecretFileOpenableCommand(dte, GetFilePath());
                    break;
            }
            return command;
        }

        private string GetFilePath()
        {
            return GetUserSecretFilePath(this.userSecretId);
        }

        private static string GetUserSecretFilePath(string userSecretId)
        {
            var basePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"Microsoft\UserSecrets\{userSecretId}")
                : Path.Combine(Environment.GetEnvironmentVariable("HOME"), $".microsoft/usersecrets/{userSecretId}");

            return Path.Combine(basePath, fileName);
        }
    }
}