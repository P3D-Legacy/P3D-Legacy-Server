using Microsoft.Extensions.Logging;

using NStack;

using P3D.Legacy.Common.PlayerEvents;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class ChatTabView : View,
        IEventHandler<PlayerJoinedEvent>,
        IEventHandler<PlayerLeftEvent>,
        IEventHandler<PlayerSentGlobalMessageEvent>,
        IEventHandler<ServerMessageEvent>,
        IEventHandler<PlayerTriggeredEventEvent>,
        IEventHandler<MessageToPlayerEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventDispatcher _eventDispatcher;

        private readonly Action<ustring> _appendChatText;

        [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
        public ChatTabView(ILogger<ChatTabView> logger, IEventDispatcher eventDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));


            Width = Dim.Fill();
            Height = Dim.Fill();

            var messageView = new FrameView("Messages") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 3 };
            var chatTextView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), ReadOnly = true, WordWrap = false };
            messageView.Add(chatTextView);
            _appendChatText = text => chatTextView.Text = text + chatTextView.Text;

            var sendMessageView = new FrameView("Send Message") { X = 0, Y = Pos.Bottom(messageView), Width = Dim.Fill(), Height = 3 };
            var buttonSendCommand = new Button("S_end", true) { X = 0, Y = 0, Height = 1 };

            var commandTextField = new TextField { X = Pos.Right(buttonSendCommand) + 1, Y = 0, Width = Dim.Fill(), Height = 1 };
            sendMessageView.Add(commandTextField, buttonSendCommand);

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            buttonSendCommand.Clicked += async () =>
            {
                try
                {
                    var message = commandTextField.Text.ToString() ?? string.Empty;
                    commandTextField.Text = ustring.Empty;
                    await HandleMessageAsync(message);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception on SendMessage.Click!");
                }
            };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

            Add(messageView, sendMessageView);
        }

        private async Task HandleMessageAsync(string message)
        {
            if (message.StartsWith("/", StringComparison.Ordinal))
            {
                await _eventDispatcher.DispatchAsync(new PlayerSentCommandEvent(IPlayer.Server, message), CancellationToken.None);
            }
            //else if (!string.IsNullOrEmpty(message))
            //{
            //    await _eventDispatcher.Dispatch(new PlayerSentGlobalMessageEvent(IPlayer.Server, message), CancellationToken.None);
            //}
        }

        public Task HandleAsync(IReceiveContext<PlayerJoinedEvent> context, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Player {context.Message.Player.Name} joined the server!{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }

        public Task HandleAsync(IReceiveContext<PlayerLeftEvent> context, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Player {context.Message.Name} left the server!{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }

        public Task HandleAsync(IReceiveContext<PlayerSentGlobalMessageEvent> context, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* {context.Message.Player.Name}: {context.Message.Message}{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }

        public Task HandleAsync(IReceiveContext<ServerMessageEvent> context, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* Server: {context.Message.Message}{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }

        public Task HandleAsync(IReceiveContext<PlayerTriggeredEventEvent> context, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* The player {context.Message.Player.Name} {PlayerEventParser.AsText(context.Message.Event)}{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }

        public Task HandleAsync(IReceiveContext<MessageToPlayerEvent> context, CancellationToken ct)
        {
            if (context.Message.To != IPlayer.Server) return Task.CompletedTask;
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                var message = $"* {(context.Message.From != IPlayer.Server ? $"{context.Message.From.Name}: " : string.Empty)}{context.Message.Message}{Environment.NewLine}";
                _appendChatText(message);
            });
            return Task.CompletedTask;
        }
    }
}