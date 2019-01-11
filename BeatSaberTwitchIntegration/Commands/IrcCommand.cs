using AsyncTwitch;
using UnityEngine;

namespace TwitchIntegrationPlugin.Commands
{
    public abstract class IrcCommand : MonoBehaviour
    {
        public abstract string[] CommandAlias { get; }
        public abstract void Run(TwitchMessage msg);
    }
}
