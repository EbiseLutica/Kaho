using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using BotBone.Core;
using BotBone.Core.Api;
using BotBone.Core.Modules;

namespace Citrine.Core.Modules
{
	public class BirthdayModule : ModuleBase
	{
		public BirthdayModule()
		{
			timer = new Timer(1000)
			{
				AutoReset = true,
				Enabled = true,
			};
			timer.Elapsed += OnTick;
		}

		public override async Task<bool> ActivateAsync(IPost n, IShell shell, Server core)
		{
			if (n.Text == null)
				return false;

			var storage = core.Storage[n.User];

			var m = patternQueryBirthday.Match(n.Text);

			if (m.Success)
			{
				var birthday = storage.Get(StorageKey.Birthday, DateTime.MinValue);
				var output = birthday == DateTime.MinValue ? "知らないよ？" : $"{birthday:yyyy年MM月dd日}だよね？";
				await shell.ReplyAsync(n, $"{core.GetNicknameOf(n.User)}の誕生日は" + output);
				return true;
			}

			m = patternSetBirthday.Match(n.Text);
			if (m.Success)
			{
				// 嫌いな人は相手にしない
				if (core.GetRatingOf(n.User) <= Rating.Hate)
					return false;

				await SetBirthday(n, shell, core, m.Groups[1].Value);
				return true;
			}

			m = patternStartBirthdayRegister.Match(n.Text);
			if (m.Success)
			{
				// 嫌いな人は相手にしない
				if (core.GetRatingOf(n.User) <= Rating.Hate)
					return false;

				var res = await shell.ReplyAsync(n, "いいよ！！誕生日の日付を教えてね。例: 1998年11月29日");
				if (res != null)
					core.RegisterContext(res, this, null);
				return true;
			}

			return false;
		}

		public override async Task<bool> OnTimelineAsync(IPost n, IShell shell, Server core)
		{
			(this.core, this.shell) = (core, shell);
			await Task.Delay(0);
			return false;
		}

		public override async Task<bool> OnRepliedContextually(IPost n, IPost? context, Dictionary<string, object> store, IShell shell, Server core)
		{
			if (n.Text == null)
				return false;

			var m = patternBirthday.Match(n.Text);

			if (!m.Success)
			{
				await shell.ReplyAsync(n, "ごめん, 正しい日付じゃないよそれ...");
				return true;
			}

			await SetBirthday(n, shell, core, m.Groups[1].Value);
			return true;
		}
		private async void OnTick(object sender, ElapsedEventArgs e)
		{
			if (core == null || shell == null)
				return;

			// 祝う対象を抽出する
			var birthDayPeople = core.Storage.Records.Where(kv =>
			{
				var (userId, storage) = kv;

				// 好感度が Like 以上
				var isLike = core.GetRatingOf(userId) >= Rating.Like;

				// 本日が誕生日である
				var birthday = storage.Get(StorageKey.Birthday, DateTime.MinValue);
				var today = DateTime.Today;
				var birthdayIsToday = birthday.Month == today.Month && birthday.Day == today.Day;

				// まだ祝ってない
				var isNotCelebratedYet = storage.Get(keyLastCelebratedYear, 0) != today.Year;

				return isLike && birthdayIsToday && isNotCelebratedYet;
			});

			foreach (var (id, storage) in birthDayPeople)
			{
				var user = await shell.GetUserAsync(id);
				if (user == null) continue;

				await shell.SendDirectMessageAsync(user, $"{core.GetNicknameOf(user)}、誕生日おめでとう！🎂");
				storage.Set(keyLastCelebratedYear, DateTime.Today.Year);
			}

		}

		private async Task SetBirthday(IPost n, IShell shell, Server core, string value)
		{
			var storage = core.Storage[n.User];
			try
			{
				var birthday = DateTime.Parse(value);
				storage.Set(StorageKey.Birthday, birthday);
				await shell.ReplyAsync(n, "覚えたよ〜！");
			}
			catch (FormatException)
			{
				await shell.ReplyAsync(n, "んー、正しい日付じゃないよ？");
			}
		}

		private const string date = @"(\d{1,4}[年/\-]\d{1,2}[月/\-]\d{1,2}[日/\-]?)";
		private static readonly Regex patternBirthday = new Regex(date);
		private static readonly Regex patternSetBirthday = new Regex($"誕生日は{date}");
		private static readonly Regex patternStartBirthdayRegister = new Regex("誕生日を?(覚|おぼ)");
		private static readonly Regex patternQueryBirthday = new Regex("誕生日(わか|分か|知って)");
		private static readonly string keyLastCelebratedYear = "birthday.last-celebrated";

		private readonly Timer timer;
		private Server? core;
		private IShell? shell;
	}
}
