using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XIVThirdEye.Core;
using XIVThirdEye.UI;

namespace XIVThirdEye
{
    public sealed class XIVThirdEyePlugin : IDalamudPlugin
    {
        public string Name => "Third Eye";
        private const string CommandName = "/thirdeye";

        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ICommandManager _commandManager;

        public Configuration Configuration { get; private set; }
        private MainWindow MainWindow { get; set; }
        private WindowSystem WindowSystem = new("XIVThirdEye");

        public XIVThirdEyePlugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IPluginLog pluginLog,
            IChatGui chatGui,
            IObjectTable objectTable,
            IClientState clientState)
        {
            _pluginInterface = pluginInterface;
            _commandManager = commandManager;

            pluginLog.Info("[ThirdEye] Plugin loaded.");

            Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(pluginInterface);

            MainWindow = new MainWindow(Configuration, objectTable, clientState, pluginLog, chatGui);
            WindowSystem.AddWindow(MainWindow);

            commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Third Eye — list players in your current instance"
            });

            pluginInterface.UiBuilder.Draw += DrawUI;
            pluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
            pluginInterface.UiBuilder.OpenConfigUi += OpenMainUI;
        }

        public void Dispose()
        {
            _pluginInterface.UiBuilder.Draw -= DrawUI;
            _pluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
            _pluginInterface.UiBuilder.OpenConfigUi -= OpenMainUI;

            _commandManager.RemoveHandler(CommandName);
            WindowSystem.RemoveAllWindows();
            MainWindow.Dispose();
        }

        private void DrawUI() => WindowSystem.Draw();
        private void OpenMainUI() => MainWindow.IsOpen = true;

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = !MainWindow.IsOpen;
        }
    }
}
