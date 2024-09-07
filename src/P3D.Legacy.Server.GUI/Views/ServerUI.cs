using System;
using System.Diagnostics.CodeAnalysis;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views;

public sealed class ServerUI : Toplevel
{
    public event EventHandler? OnStop;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public ServerUI(PlayerTabView playerTabView, ChatTabView chatTabView, LogsTabView logsTabView, SettingsTabView settingsTabView)
    {
        var win = new Window("Server Management Panel") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 1 };
        var tabView = new TabView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(new TabView.Tab("Players", playerTabView), andSelect: true);
        tabView.AddTab(new TabView.Tab("Chat", chatTabView), andSelect: false);
        tabView.AddTab(new TabView.Tab("Logs", logsTabView), andSelect: false);
        tabView.AddTab(new TabView.Tab("Settings", settingsTabView), andSelect: false);
        win.Add(tabView);

        var statusBar = new StatusBar([
            new(Key.F1, "~F1~ Help", static () => MessageBox.Query(50, 7, "Help", "Helping", "Ok")),
            new(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => OnStop?.Invoke(this, EventArgs.Empty)),
            new(Key.Null, Terminal.Gui.Application.Driver.GetType().Name, null)
        ]);

        Add(win, statusBar);
    }
}