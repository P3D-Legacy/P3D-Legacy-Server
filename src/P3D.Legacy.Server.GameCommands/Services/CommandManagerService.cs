using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.GameCommands.Managers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using PCLExt.Config;

using PokeD.Core;
using PokeD.Core.Data;
using PokeD.Core.Packets.P3D.Shared;
using PokeD.Core.Services;
using PokeD.Server.Chat;
using PokeD.Server.Clients;
using PokeD.Server.Commands;
using PokeD.Server.Data;
using PokeD.Server.Database;
using PokeD.Server.Modules;

namespace PokeD.Server.Services
{
    public class CommandManagerService
    {
        private List<CommandManager> Commands { get; } = new();

        private bool IsDisposed { get; set; }

        public CommandManagerService(IServiceContainer services, ConfigType configType) : base(services, configType) { }

        /// <summary>
        /// Return <see langword="false"/> if <see cref="Command"/> not found.
        /// </summary>
        public bool ExecuteClientCommand(IPlayer client, string message)
        {
            var commandWithoutSlash = message.TrimStart('/');

            var messageArray = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(commandWithoutSlash).Select(str => str.TrimStart('"').TrimEnd('"')).ToArray();
            //var messageArray = commandWithoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length == 0)
                return false; // command not found

            var alias = messageArray[0];
            var trimmedMessageArray = messageArray.Skip(1).ToArray();

            if (!Commands.Any(c => c.Name == alias || c.Aliases.Any(a => a == alias)))
                return false; // command not found

            HandleCommand(client, alias, trimmedMessageArray);

            return true;
        }

        /// <summary>
        /// Return <see langword="false"/> if <see cref="Command"/> not found.
        /// </summary>
        public bool ExecuteServerCommand(string message) => ExecuteClientCommand(new ServerClient(null), message);

        private void HandleCommand(IPlayer client, string alias, string[] arguments)
        {
            var command = FindByName(alias) ?? FindByAlias(alias);
            if (command == null)
            {
                client.SendServerMessage($@"Invalid command ""{alias}"".");
                return;
            }

            if(command.LogCommand && (client.Permissions & PermissionFlags.UnVerified) == 0)
                Logger.LogCommandMessage(client.Name, $"/{alias} {string.Join(" ", arguments)}");

            if (command.Permissions == PermissionFlags.None)
            {
                client.SendServerMessage(@"Command is disabled!");
                return;
            }

            if ((client.Permissions & command.Permissions) == PermissionFlags.None)
            {
                client.SendServerMessage(@"You have not the permission to use this command!");
                return;
            }

            command.Handle(client, alias, arguments);
        }

        public CommandManager FindByName(string name) => Commands.Find(command => command.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public CommandManager FindByAlias(string alias) => Commands.Find(command => command.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));

        public IReadOnlyList<CommandManager> GetCommands() => Commands;


        public override bool Start()
        {
            Logger.Log(LogType.Debug, "Loading Commands...");
            LoadCommands();
            Logger.Log(LogType.Debug, "Loaded Commands.");
            return true;
        }
        public override bool Stop()
        {
            Logger.Log(LogType.Debug, "Unloading Commands...");
            Commands.Clear();
            Logger.Log(LogType.Debug, "Unloaded Commands.");
            return true;
        }
        private void LoadCommands()
        {
            var types = typeof(CommandManagerService).GetTypeInfo().Assembly.DefinedTypes
                .Where(typeInfo => typeof(CommandManager).GetTypeInfo().IsAssignableFrom(typeInfo) &&
                !typeInfo.IsDefined(typeof(CommandDisableAutoLoadAttribute), true) &&
                !typeInfo.IsAbstract);

            foreach (var command in types.Where(type => !Equals(type, typeof(ScriptCommand).GetTypeInfo())).Select(type => (CommandManager) Activator.CreateInstance(type.AsType(), Services)))
                Commands.Add(command);


            var scriptCommandLoaderTypes = typeof(CommandManagerService).GetTypeInfo().Assembly.DefinedTypes
                .Where(typeInfo => typeof(ScriptCommandLoader).GetTypeInfo().IsAssignableFrom(typeInfo) &&
                !typeInfo.IsDefined(typeof(CommandDisableAutoLoadAttribute), true) &&
                !typeInfo.IsAbstract);

            foreach (var scriptCommandLoader in scriptCommandLoaderTypes.Where(type => type != typeof(ScriptCommandLoader).GetTypeInfo()).Select(type => (ScriptCommandLoader) Activator.CreateInstance(type.AsType())))
                Commands.AddRange(scriptCommandLoader.LoadCommands(Services));
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Commands.Clear();
                }


                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}