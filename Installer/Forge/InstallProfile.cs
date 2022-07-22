using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakaXL.GameCores.Installer.Forge {


	public class InstallProfile {

		[JsonPropertyName("spec")]
		public int spec;

		[JsonPropertyName("profile")]
		public string profile;

		[JsonPropertyName("version")]
		public string version;

		[JsonPropertyName("json")]
		public string json;

		[JsonPropertyName("path")]
		public string path;

		[JsonPropertyName("minecraft")]
		public string minecraft;

		[JsonPropertyName("data")]
		public Dictionary<string, InstallProfileDataContent> data;

		[JsonPropertyName("processors")]
		public List<InstallProfileProcessors> processors;

		[JsonPropertyName("libraries")]
		public List<ULauncherCore.Jsons.JsonLibrary> libraries;
	}

	public class InstallProfileDataContent {

		[JsonPropertyName("client")]
		public string client;

		[JsonPropertyName("server")]
		public string server;
	}

	public class InstallProfileProcessors {

		[JsonPropertyName("sides")]
		public List<string> sides;

		[JsonPropertyName("jar")]
		public string jar;

		[JsonPropertyName("classpath")]
		public List<string> classpath;

		[JsonPropertyName("args")]
		public List<string> args;

		[JsonPropertyName("outputs")]
		public List<string> outputs;
	}
}
