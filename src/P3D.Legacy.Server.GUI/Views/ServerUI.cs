using System;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class ServerUI : Toplevel
    {
        public event EventHandler? OnStop; 

        public ServerUI(PlayerTabView playerTabView, ChatTabView chatTabView, LogsTabView logsTabView, SettingsTabView settingsTabView)
        {
            var win = new Window("Server Management Panel") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 1 };
            var tabView = new TabView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            tabView.AddTab(new TabView.Tab("Players", playerTabView), true);
            tabView.AddTab(new TabView.Tab("Chat", chatTabView), false);
            tabView.AddTab(new TabView.Tab("Logs", logsTabView), false);
            tabView.AddTab(new TabView.Tab("Settings", settingsTabView), false);
            win.Add(tabView);

            var statusBar = new StatusBar(new StatusItem[]
            {
                new(Key.F1, "~F1~ Help", () => MessageBox.Query(50, 7, "Help", "Helping", "Ok")),
                new(Key.CtrlMask | Key.Q, "~^Q~ Quit", () =>
                {
                    OnStop?.Invoke(this, EventArgs.Empty);
                }),
                new(Key.Null, Terminal.Gui.Application.Driver.GetType().Name, null)
            });

            Add(win, statusBar);
        }
    }
}