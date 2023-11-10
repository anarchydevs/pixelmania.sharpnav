using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SharpNav;
using System;

namespace Test
{
    public class Main : AOPluginEntry
    {
        public override void Run(string pluginDir)
        {
            Chat.WriteLine("Sharp Nav Test Build");

            Game.OnUpdate += OnUpdate;
        }

        private void OnUpdate(object sender, float e)
        {
            SMovementController.Instance?.Update(sender, e);
        }
    }
}