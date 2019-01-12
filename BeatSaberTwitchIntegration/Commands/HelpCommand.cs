using AsyncTwitch;

namespace TwitchIntegrationPlugin.Commands
{
    class HelpCommand : IrcCommand
    {
        public override string[] CommandAlias => new[] { "qhelp" };

        public override void Run(TwitchMessage msg)
        {
            TwitchConnection.Instance.SendChatMessage("These are the valid commands for the Beat Saber Queue system.");
            TwitchConnection.Instance.SendChatMessage("[Everyone] !bsr <songname/id>, !add <songname/id>, !queue, !blist, !pat");
            TwitchConnection.Instance.SendChatMessage("[Mod only] !next, !clearall, !block <songId>, !unblock <songId>, !open, !close, !randomize, !saveq, !clearusers, !mtt <songId or username>");
        }
    }
}
