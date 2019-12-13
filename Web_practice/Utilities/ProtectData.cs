using Microsoft.AspNetCore.DataProtection;
using System;

namespace WebVisualGame_MVC.Utilities
{
	public class ProtectData
	{
		private static ProtectData instance;

		private static IDataProtector _protector;

		public void Initialize(IDataProtectionProvider provider)
		{
			_protector = provider.CreateProtector(GetType().FullName);
		}

		public static ProtectData GetInstance()
		{
			if (instance == null)
				instance = new ProtectData();
			return instance;
		}

		public string Encode(string value) => _protector.Protect(value);
		public string Encode(int value) => _protector.Protect(value.ToString());

		public int DecodeToInt(string key) => Convert.ToInt32(_protector.Unprotect(key));
		public string DecodeToString(string key) => _protector.Unprotect(key);
	}
}