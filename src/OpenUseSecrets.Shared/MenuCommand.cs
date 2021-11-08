using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OpenUserSecrets
{
    /// <summary>
    /// Command handler
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    internal sealed class ProjectContextMenuCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0b75bc9c-2cd0-4ccf-9ed2-59d1ad08f47b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private EnvDTE.DTE _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectContextMenuCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ProjectContextMenuCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ProjectContextMenuCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MenuCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ProjectContextMenuCommand(package, commandService);
            Instance._dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_dte == null)
                return;

            var solution = _dte.Solution;
            var project = (EnvDTE.Project)((object[])_dte.ActiveSolutionProjects)[0];
            var active = project.ConfigurationManager.ActiveConfiguration;
            var fullPath = project.FullName;
            if (File.Exists(fullPath))
            {
                try
                {
                    var stateMachine = new UserSecretStateMachine(fullPath, "UserSecretsId", this._dte, this.package);
                    // state still has next.
                    while (stateMachine.MoveNext())
                    {
                        _dte.PrintMessageLine($"current state: {stateMachine.Current}");
                        stateMachine.Command?.Execute();
                    }

                    // final command
                    _dte.PrintMessageLine($"current state: {stateMachine.Current}");
                    stateMachine.Command?.Execute();
                }
                catch (Exception ex)
                {
                    Debug.Print($"{ex.GetType().FullName}, {ex.Message}, {ex.StackTrace}");
                }
            }
        }
    }
}
