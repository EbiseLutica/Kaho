using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using BotBone.Core;
using BotBone.Core.Api;
using BotBone.Core.Modules;
using Newtonsoft.Json;

namespace Kaho.Modules
{
	public class HelloWorldModule : ModuleBase
	{
		public HelloWorldModule()
		{
			var timer = new Timer(1000);
			timer.Elapsed += async (_, _) =>
			{
				if (Server.Current is Server s)
				{
					timer.Stop();
					var storage = s.GetMyStorage();
					if (storage.Has("kaho.greeted")) return;
					await s.Shell.PostAsync("アカウント作りました！よろしくね");
					storage.Set("kaho.greeted", true);
				}
			};
			timer.Start();
		}
	}
}
