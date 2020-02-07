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
		public List<JsonLibrary> libraries;
	}

	public class InstallProfileDataContent {

		[JsonPropertyName("client")]
		public string client;

		[JsonPropertyName("server")]
		public string server;
	}

	public class InstallProfileProcessors {

		[JsonPropertyName("jar")]
		public string jar;

		[JsonPropertyName("classpath")]
		public List<string> classpath;

		[JsonPropertyName("args")]
		public List<string> args;

		[JsonPropertyName("outputs")]
		public List<string> outputs;
	}

	public sealed class JsonLibrary {

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("url")]
		public string URL { get; set; }

		[JsonPropertyName("natives")]
		public Dictionary<string, string> Natives { get; set; }

		[JsonPropertyName("rules")]
		public List<JsonRule> Rules { get; set; }

		[JsonPropertyName("extract")]
		public JsonExtract Extract { get; set; }

		[JsonPropertyName("checksums")]
		public List<string> Checksums { get; set; }

		[JsonPropertyName("downloads")]
		public JsonDownloads Downloads { get; set; }

		[JsonPropertyName("clientreq")]
		public bool IsClientRequirement { get; set; } = true;

	}

	public sealed class JsonRule {

		[JsonPropertyName("action")]
		public string Action { get; set; }

		[JsonPropertyName("os")]
		public JsonOperatingSystem OS { get; set; }

	}

	public sealed class JsonOperatingSystem {

		[JsonPropertyName("name")]
		public string Name { get; set; }

	}

	public sealed class JsonExtract {

		[JsonPropertyName("exclude")]
		public List<string> Exclude { get; set; }

	}

	public sealed class JsonDownloads {

		[JsonPropertyName("artifact")]
		public JsonDownload Artifact { get; set; }

		[JsonPropertyName("classifiers")]
		public Dictionary<string, JsonDownload> Classifiers { get; set; }

	}

	public sealed class JsonDownload {

		[JsonPropertyName("url")]
		public string URL { get; set; }

		[JsonPropertyName("sha1")]
		public string Hash { get; set; }

		[JsonPropertyName("size")]
		public int Size { get; set; }

		[JsonPropertyName("path")]
		public string Path { get; set; }

	}
}
