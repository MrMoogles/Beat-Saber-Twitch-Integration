using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsyncTwitch;
using TwitchIntegrationPlugin.Commands;
using Logger = TwitchIntegrationPlugin.Misc.Logger;

namespace TwitchIntegrationPlugin
{
    public class BeatBotNew
    {
        private const string Prefix = "!";
        private Dictionary<string, IrcCommand> _commandDict = new Dictionary<string, IrcCommand>();

        public BeatBotNew()
        {
            StaticData.Config = StaticData.Config.LoadFromJson();
            StaticData.SongQueue.LoadSongQueue();
            //StaticData.SongQueue.CreatePlaylist();
            StaticData.BanList.LoadBanList("UserData/TwitchIntegrationBans.json");

            // Differs from normal Ban list in that only the owner can add songs for certain events
            // Doesn't actually display any messages in Chat
            if (StaticData.Config.EnableShadowQueue){
                StaticData.ShadowBanList.LoadBanList("UserData/TwitchIntegrationShadowBans.json");
            }

            LoadCommandClasses();
            PrintConfigValuesToLog();

            TwitchConnection.Instance.StartConnection();
            TwitchConnection.Instance.RegisterOnMessageReceived(OnMessageReceived);
        }

        private void OnMessageReceived(TwitchConnection connection, TwitchMessage msg)
        {
            if (!msg.Content.StartsWith(Prefix)) return;
            string commandString = msg.Content.Split(' ')[0];
            commandString = commandString.Remove(0, Prefix.Length);

            if (_commandDict.ContainsKey(commandString))
            {
                _commandDict[commandString].Run(msg);
            }
        }

        private void LoadCommandClasses()
        {
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            IEnumerable<Type> commandList = assemblyTypes.Where(x => x.IsSubclassOf(typeof(IrcCommand)));

            foreach (Type abstractCommand in commandList)
            {
                IrcCommand command = (IrcCommand)Activator.CreateInstance(abstractCommand);
                foreach (string alias in command.CommandAlias)
                {
                    _commandDict.Add(alias, command);
                }
            }
        }

        private void PrintConfigValuesToLog()
        {
            Logger.Log("Queue is Mod Only? " + StaticData.Config.ModOnly);
            Logger.Log("Queue is Sub Only? " + StaticData.Config.SubOnly);
            Logger.Log("Viewer Request Limits: " + StaticData.Config.ViewerLimit);
            Logger.Log("Sub Request Limits: " + StaticData.Config.SubLimit);
            Logger.Log("Moderator Request Limits: " + StaticData.Config.ModLimit);
            Logger.Log("Randomize Allowed? " + StaticData.Config.Randomize);
            Logger.Log("Randomize Request Limit: " + StaticData.Config.RandomizeLimit);
            Logger.Log("Request Multiple Random Queues already Chosen: " + StaticData.Config.BlockMultiRandomQueue);
            Logger.Log("Request Songs Already Played in Earlier Queues: " + StaticData.Config.OverrideSongInMultiQueue);
            Logger.Log("Shadow Queue Enabled? " + StaticData.Config.EnableShadowQueue);
            Logger.Log("Disable Mod Override: " + StaticData.Config.DisableModOverride);
            Logger.Log("Disable Return Chat Messages: " + StaticData.Config.AllowTwitchResponses);
        }
    }
}
