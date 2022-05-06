using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class LogsTabView : View
    {
        private readonly TextView _logsTextView;

        [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
        public LogsTabView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();

            var messageView = new FrameView("Logs") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _logsTextView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), ReadOnly = true, WordWrap = false };
            messageView.Add(_logsTextView);

            Add(messageView);
        }

        public void Log<TState>(string category, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            var content = message + Environment.NewLine + _logsTextView.Text;

            const int maxLines = 500;
            var toDelete = content.Count(Environment.NewLine) - maxLines;
            while (toDelete > 0)
            {
                content = content.RuneSubstring(_logsTextView.Text.LastIndexOf(Environment.NewLine));
                toDelete--;
            }

            _logsTextView.Text = content;
        }
    }
}