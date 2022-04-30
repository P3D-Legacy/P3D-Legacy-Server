using Microsoft.Extensions.Logging;

using NStack;

using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class ChatTabView : View,
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>,
        INotificationHandler<PlayerSentGlobalMessageNotification>,
        INotificationHandler<ServerMessageNotification>,
        INotificationHandler<PlayerTriggeredEventNotification>,
        INotificationHandler<MessageToPlayerNotification>
    {
        private readonly ILogger _logger;
        private readonly INotificationDispatcher _notificationDispatcher;

        private readonly TextView _chatTextView;
        private readonly TextField _commandTextField;

        public ChatTabView(ILogger<ChatTabView> logger, INotificationDispatcher notificationDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));


            Width = Dim.Fill();
            Height = Dim.Fill();

            var messageView = new FrameView("Messages") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 3 };
            _chatTextView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), ReadOnly = true, WordWrap = false };
            messageView.Add(_chatTextView);

            var sendMessageView = new FrameView("Send Message") { X = 0, Y = Pos.Bottom(messageView), Width = Dim.Fill(), Height = 3 };
            var buttonSendCommand = new Button("S_end", true) { X = 0, Y = 0, Height = 1 };
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            buttonSendCommand.Clicked += async () =>
            {
                try
                {
                    if (_commandTextField is null) return;
                    var message = _commandTextField.Text.ToString() ?? string.Empty;
                    _commandTextField.Text = ustring.Empty;
                    await HandleMessageAsync(message);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception on SendMessage.Click!");
                }
            };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            _commandTextField = new TextField { X = Pos.Right(buttonSendCommand) + 1, Y = 0, Width = Dim.Fill(), Height = 1 };
            sendMessageView.Add(_commandTextField, buttonSendCommand);

            Add(messageView, sendMessageView);
        }

        private async Task HandleMessageAsync(string message)
        {
            if (message.StartsWith("/", StringComparison.Ordinal))
            {
                await _notificationDispatcher.DispatchAsync(new PlayerSentCommandNotification(IPlayer.Server, message), CancellationToken.None);
            }
            //else if (!string.IsNullOrEmpty(message))
            //{
            //    await _notificationDispatcher.Dispatch(new PlayerSentGlobalMessageNotification(IPlayer.Server, message), CancellationToken.None);
            //}
        }

        public Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Player {notification.Player.Name} joined the server!{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }

        public Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Player {notification.Name} left the server!{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }

        public Task Handle(PlayerSentGlobalMessageNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* {notification.Player.Name}: {notification.Message}{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }

        public Task Handle(ServerMessageNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Server: {notification.Message}{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }

        public Task Handle(PlayerTriggeredEventNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* The player {notification.Player.Name} {PlayerEventParser.AsText(notification.Event)}{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }

        public Task Handle(MessageToPlayerNotification notification, CancellationToken ct)
        {
            if (notification.To != IPlayer.Server) return Task.CompletedTask;
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* {(notification.From != IPlayer.Server ? $"{notification.From.Name}: " : string.Empty)}{notification.Message}{Environment.NewLine}";
                _chatTextView.Text = message + _chatTextView.Text;
            });
            return Task.CompletedTask;
        }
    }
}