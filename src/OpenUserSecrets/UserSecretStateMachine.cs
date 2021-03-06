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
            UserSecretsByWeb,
            UserSecretsEntryNotExists,
            UserSecretFileNotExists,
            UserSecretFileOpenable,
        }

        private static readonly string fileName = "secrets.json";
        private static readonly string requiredPackage = "Microsoft.Extensions.Configuration.UserSecrets";

        public ICommand Command { get; private set; }
        public State Current { get; private set; } = State.UserSecretsEntryNotExists;

        private readonly ProjectCollection collection;
        private readonly string path;
        private readonly string propertyKey;
        private readonly EnvDTE.DTE dte;
        private readonly Microsoft.VisualStudio.Shell.AsyncPackage package;
        private string userSecretId;
        private ICommand subCommand;

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
                case State.UserSecretsEntryNotExists:
                    return true;
                case State.UserSecretFileNotExists:
                    return true;
                case State.UserSecretFileOpenable:
                    return false;
                case State.UserSecretsByWeb:
                    return false;
            }
            return false;
        }

        private void UpdateState()
        {
            collection.UnloadAllProjects();
            collection.LoadProject(path);

            var csproj = collection.GetLoadedProjects(path).FirstOrDefault();
            var isWebProject = csproj.Xml.Sdk == "Microsoft.NET.Sdk.Web"; // Special project type which include UserSecrets in SDK.
            var packageExists = csproj.Items
                .Where(x => x.ItemType == "PackageReference")
                .Where(x => x.EvaluatedInclude == requiredPackage)
                .Any();
            this.userSecretId = csproj.GetPropertyValue(propertyKey);

            // subcommand
            if (isWebProject)
            {
                this.subCommand = new WebProjectCommand(dte, csproj.Xml.Sdk);
            }
            else if (!packageExists)
            {
                this.subCommand = new MissingPackageCommand(dte, requiredPackage);
            }
            else
            {
                this.subCommand = null;
            }

            // state
            if (isWebProject)
            {
                Current = State.UserSecretsByWeb;
            }
            if (string.IsNullOrWhiteSpace(this.userSecretId))
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
                case State.UserSecretsEntryNotExists:
                    command = new UserSecretsEntryNotExistsCommand(propertyKey, path, subCommand);
                    break;
                case State.UserSecretFileNotExists:
                    command = new UserSecretFileNotExistsCommand(GetFilePath(), subCommand);
                    break;
                case State.UserSecretFileOpenable:
                    command = new UserSecretFileOpenableCommand(dte, GetFilePath(), subCommand);
                    break;
                case State.UserSecretsByWeb:
                    command = new UserSecretFileOpenableCommand(dte, GetFilePath(), subCommand);
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
