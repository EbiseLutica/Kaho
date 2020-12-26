using System.Threading.Tasks;
using BotBone.Core;
using BotBone.Core.Api;
using BotBone.Core.Modules;

namespace Kaho.Modules
{
	public class CommandModule : ModuleBase
	{
		public override int Priority => -10000;
		public static readonly string StatCommandUsedCount = "stat.command.used-count";
		public override async Task<bool> ActivateAsync(IPost n, IShell shell, Server core)
		{
			var t = n.Text?.TrimMentions();
			if (string.IsNullOrEmpty(t))
				return false;
			if (t.StartsWith("/"))
			{
				string response;
				try
				{
					response = await core.ExecCommand(new PostCommandSender(n, core.IsSuperUser(n.User)), t);
					core.Storage[n.User].Add(StatCommandUsedCount);
				}
				catch (AdminOnlyException)
				{
					response = "あー、このコマンドは管理者限定なんです！ごめんね";
				}
				catch (LocalOnlyException)
				{
					response = "あー、このコマンドは同じインスタンスのユーザーしか使えないの！！ごめんね";
				}
				catch (RemoteOnlyException)
				{
					response = "あー、このコマンドは違うインスタンスのユーザーしか使えないの！！ごめんね";
				}
				catch (NoSuchCommandException)
				{
					response = $"No such command.";
				}

				if (!string.IsNullOrWhiteSpace(response))
				{
					await shell.ReplyAsync(n, response);
				}
				EconomyModule.Pay(n, shell, core);
				return true;
			}
			return false;
		}
	}
}
