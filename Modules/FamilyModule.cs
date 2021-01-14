#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
using System.Threading.Tasks;
using BotBone.Core;
using BotBone.Core.Api;
using BotBone.Core.Modules;

namespace Kaho.Modules
{
	public class FamilyModule : ModuleBase
	{
		public override int Priority => -10004;

		public override async Task<bool> ActivateAsync(IPost n, IShell shell, Server core)
		{
			if (n.Text == null) return false;
			if (string.IsNullOrEmpty(n.User.Host) && n.User.Name.ToLowerInvariant() == "citrine")
			{
				await shell.ReactAsync(n, "❤️");
				return true;
			}
			return false;
		}
	}
}
