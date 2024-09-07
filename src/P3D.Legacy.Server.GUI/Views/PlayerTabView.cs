using Microsoft.Extensions.Logging;

using NStack;

using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Commands.Administration;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Events.Player;
using P3D.Legacy.Server.Domain.Services;
using P3D.Legacy.Server.GUI.Utils;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views;

public sealed class PlayerTabView : View,
    IEventHandler<PlayerJoinedEvent>,
    IEventHandler<PlayerLeftEvent>
{
    private readonly ILogger _logger;
    private readonly ICommandDispatcher _commandDispatcher;

#pragma warning disable IDISP006
    private readonly ListView _playerListView;
    private readonly TextView _playerInfoTextView;
    private readonly Button _kickButton;
    private readonly Button _banButton;
#pragma warning restore IDISP006

    private readonly PlayerListDataSource _currentPlayers = new([]);
    private IPlayer? _selectedPlayer;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public PlayerTabView(ILogger<PlayerTabView> logger, ICommandDispatcher commandDispatcher, IPlayerContainerReader playerContainer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));


        Width = Dim.Fill();
        Height = Dim.Fill();

        var onlineView = new FrameView("Online") { X = 0, Y = 0, Width = 20, Height = Dim.Fill() };
        _currentPlayers.Players.AddRange(playerContainer.GetAll().Where(static x => x.Permissions > PermissionTypes.UnVerified));
        _playerListView = new ListView(_currentPlayers) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        onlineView.Add(_playerListView);

        var infoView = new FrameView("Info") { X = 20, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        _playerInfoTextView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = 7, ReadOnly = true };
        _kickButton = new Button("Kick") { X = 0, Y = Pos.Bottom(_playerInfoTextView), Height = 1, Visible = false };
        _banButton = new Button("Ban") { X = Pos.Right(_kickButton), Y = Pos.Bottom(_playerInfoTextView), Height = 1, Visible = false };
        infoView.Add(_playerInfoTextView, _kickButton, _banButton);

        _playerListView.OpenSelectedItem += args =>
        {
            if (args.Value is IPlayer player)
            {
                _selectedPlayer = player;

                _kickButton.Visible = true;
                _banButton.Visible = true;

                _playerInfoTextView.Text = $"""
Connection Id: {player.ConnectionId}
Id: {player.Id}
Origin: {player.Origin}
Name: {player.Name}
Permissions: {player.Permissions}
IP: {player.IPEndPoint}
""";
            }
            else
            {
                RemovePlayerInfo();
            }
        };
        _kickButton.Clicked += () =>
        {
            if (_selectedPlayer is not null)
#pragma warning disable IDISP004
                Terminal.Gui.Application.Run(Kick(_selectedPlayer));
#pragma warning restore IDISP004
        };
        _banButton.Clicked += () =>
        {
            if (_selectedPlayer is not null)
#pragma warning disable IDISP004
                Terminal.Gui.Application.Run(Ban(_selectedPlayer));
#pragma warning restore IDISP004
        };

        Add(onlineView, infoView);
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    private Dialog Kick(IPlayer player)
    {
        var dialog = new Dialog("Kick", 40, 6);
        var reasonInfoFrameView = new FrameView("Write a reason for kicking:") { X = 0, Y = 0, Width = Dim.Fill(), Height = 3 };
        var reasonTextField = new TextField { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        reasonInfoFrameView.Add(reasonTextField);
        var kick = new Button("Kick") { X = 0, Y = Pos.AnchorEnd() - 1 };
        var cancel = new Button("Cancel") { X = Pos.AnchorEnd() - "Cancel".Length - 4, Y = Pos.AnchorEnd() - 1 };
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
        kick.Clicked += async () =>
        {
            try
            {
                var reason = ustring.IsNullOrEmpty(reasonTextField.Text) ? "Kicked by a Moderator or Admin." : reasonTextField.Text.ToString() ?? string.Empty;
                RemovePlayerInfo();
                await _commandDispatcher.DispatchAsync(new KickPlayerCommand(player, reason), CancellationToken.None);
                Terminal.Gui.Application.RequestStop();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception on Kick.Click!");
            }
        };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        cancel.Clicked += static () => Terminal.Gui.Application.RequestStop();
        dialog.Add(reasonInfoFrameView, kick, cancel);
        return dialog;
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    private Dialog Ban(IPlayer player)
    {
        var dialog = new Dialog("Ban", 40, 11);
        var reasonInfoFrameView = new FrameView("Write a reason for banning:") { X = 0, Y = 0, Width = Dim.Fill(), Height = 3 + 2 };
        var reasonTextView = new ComboBox("Select a reason for banning") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        reasonTextView.Source = new ListWrapper(new[] { "Test1", "Test2", "Test3", "Test4" });
        reasonInfoFrameView.Add(reasonTextView);
        var durationInfoFrameView = new FrameView("Duration in minutes:") { X = 0, Y = Pos.Bottom(reasonInfoFrameView), Width = Dim.Fill(), Height = 3 };
        var durationTextField = new TextField { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        durationTextField.TextChanged += text =>
        {
            if (!ulong.TryParse(durationTextField.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                durationTextField.Text = ustring.Empty;
        };
        durationInfoFrameView.Add(durationTextField);

        var ban = new Button("Ban") { X = 0, Y = Pos.AnchorEnd() - 1 };
        var cancel = new Button("Cancel") { X = Pos.AnchorEnd() - "Cancel".Length - 4, Y = Pos.AnchorEnd() - 1 };
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
        ban.Clicked += async () =>
        {
            try
            {
                var reason = ustring.IsNullOrEmpty(reasonTextView.Text) ? "Kicked by a Moderator or Admin." : reasonTextView.Text.ToString() ?? string.Empty;
                RemovePlayerInfo();
                await _commandDispatcher.DispatchAsync(new KickPlayerCommand(player, reason), CancellationToken.None);
                Terminal.Gui.Application.RequestStop();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception on Ban.Click!");
            }
        };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        cancel.Clicked += static () => Terminal.Gui.Application.RequestStop();
        dialog.Add(ban, cancel, reasonInfoFrameView, durationInfoFrameView);
        return dialog;
    }

    private void RemovePlayerInfo()
    {
        _selectedPlayer = null;

        _kickButton.Visible = false;
        _banButton.Visible = false;

        _playerInfoTextView.Text = ustring.Empty;
    }

    public Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
    {
        Terminal.Gui.Application.MainLoop.Invoke(() =>
        {
            _currentPlayers.Players.Add(context.Message.Player);
            _playerListView.Source = _currentPlayers;
        });
        return Task.CompletedTask;
    }

    public Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
    {
        Terminal.Gui.Application.MainLoop.Invoke(() =>
        {
            _currentPlayers.Players.RemoveAll(x => x.Id == context.Message.Id);
            _playerListView.Source = _currentPlayers;
        });
        return Task.CompletedTask;
    }
}