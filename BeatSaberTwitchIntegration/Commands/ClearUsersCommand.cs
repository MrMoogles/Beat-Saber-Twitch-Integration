using AsyncTwitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchIntegrationPlugin.Serializables;

namespace TwitchIntegrationPlugin.Commands
{
    class ClearUsersCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "clearusers" };
        public override void Run(TwitchMessage msg)
        {
            if (!msg.Author.IsMod && !msg.Author.IsBroadcaster) return;
            StaticData.UserPickedByRandomize.Clear();
            if (StaticData.Config.AllowTwitchResponses)
                TwitchConnection.Instance.SendChatMessage("All users cleared and can request songs.");
        }
    }
}
