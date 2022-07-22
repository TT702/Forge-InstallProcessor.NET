using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakaXL.GameCores.Installer.Forge {
	class Processor {
		private static string AppWorkPath = "";
		private static LogLevel LogStatus = Processor.LogLevel.None;

		private InstallProfile InstallProfile;
		private string MINECRAFT_JAR = "";
		private string BINPATCH_PATH_CLIENT = "";

		//private IProgress<InstallTask> InstallProgress;

		private string JRE_PATH = "";

		public enum LogLevel {
			None, InstallOnly, Verbose
		}

		public bool Init(InstallProfile installProfile, string jre_path) {
			AppWorkPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			this.JRE_PATH = jre_path;

			try {
				foreach(var data in installProfile.data) {
					if (LogStatus != 0) {
						Trace.WriteLine("[Forge-InstallProcessor.NET][Info] Convert Mapping Data to Path Data.");
					}
					data.Value.client = GetPathFromPackageName(data.Value.client);
					data.Value.server = GetPathFromPackageName(data.Value.server);
				}

				BINPATCH_PATH_CLIENT = $"{PathUtils.GetPath(AppWorkPath, "BakaXL", "Temp", "ForgeInstall", "data", "client.lzma")}";
				if (!File.Exists(BINPATCH_PATH_CLIENT)) {
					//client.lzma was missing inside Forge Installer
					if (LogStatus != 0) {
						Trace.WriteLine("[Forge-InstallProcessor.NET][Error] client.lzma was missing in Forge Installer!");
					}
					throw new Exception();
				}

				if (!String.IsNullOrWhiteSpace(installProfile.minecraft)) {
					MINECRAFT_JAR = $"{PathUtils.GetPath(AppWorkPath, ".minecraft", "versions", installProfile.minecraft)}\\{installProfile.minecraft}.jar";
				} else {
					if (LogStatus != 0) {
						Trace.WriteLine("[Forge-InstallProcessor.NET][Error] install_profile.json does not contain Minecraft Version!");
					}
					//Install Profile Does not conatin Minecraft Version.
					throw new Exception();
				}

				this.InstallProfile = installProfile;
				//this.InstallProgress = installProgress;

				if (LogStatus != 0) {
					Trace.WriteLine("[Forge-InstallProcessor.NET][Info] Successfully Initialize Processor.");
				}

				return true;
			} catch (Exception ex) {
				if (LogStatus != 0) {
					Trace.WriteLine("[Forge-InstallProcessor.NET][Failed] Unsupported Forge Installer.");
					Trace.WriteLine($"[Forge-InstallProcessor.NET][Failed] {ex}");
				}
				//Unsupported Forge Installer Version.
				return false;
			}
		}

		public void Begin() {

			if (LogStatus != 0) {
				Trace.WriteLine("[Forge-InstallProcessor.NET][Info] Start Install Process.");
			}

			var progressSplit = 100 / InstallProfile.processors.Count;
			var current = 0;

			for (int i = 0; i < InstallProfile.processors.Count; i++) {
				if (LogStatus != 0) {
					Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Step #{i+1}] Prepare Processor.");
				}

				current = current + progressSplit;
				//InstallProgress.Report(new <Your Task Here>() {  });		
				
				if(InstallProfile.processors[i].sides != null) {		
					if (InstallProfile.processors[i].sides[0] == "server") {
						if (LogStatus != 0) Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Step #{i+1}] Skip Server Side Process");
						continue;
					}
				}

				if (InstallProfile.processors[i].args[1].Contains("DOWNLOAD_MOJMAPS")) {
					if (LogStatus != 0) Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Step #{i+1}] Need to Download Minecraft Client Mapping");
					DownloadMOJMAPS(InstallProfile.minecraft, InstallProfile.data["MOJMAPS"].client.Replace("\"", ""));
					continue;
				}

				if (LogStatus != 0) {
					Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Step #{i + 1}] Mixing Args.");
				}
				var args = MixArgs(InstallProfile.processors[i]);

				if (LogStatus == LogLevel.Verbose) {
					Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Verbose] Args For Step {i+1}:");
					Trace.WriteLine(args);
				}

				RunProcess(args);
			}
		}

		private void RunProcess(string args) {
			var process = new ProcessStartInfo(JRE_PATH, args) {
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			var commander = Process.Start(process);
			commander.BeginOutputReadLine();
			commander.OutputDataReceived += Commander_OutputDataReceived;
			commander.WaitForExit();
		}

		private void Commander_OutputDataReceived(object sender, DataReceivedEventArgs e) {
			var data = e.Data;
			if (data == null) return;

			if (!String.IsNullOrWhiteSpace(data) && data.Contains("Exception in thread")) {
				throw new Exception("Build Failed!");
			}
			
			if (LogStatus != 0) {
				Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][Forge-Output] {e.Data}");
			}
		}

		private string MixArgs(Forge.InstallProfileProcessors processors) {
			var args = new StringBuilder("-cp ");
			var MainClass = GetLibraryFile(processors.jar);

			//Jar Path
			args.Append($"\"{MainClass}\";");

			//Class Path
			for (var i = 0; i < processors.classpath.Count;i++) {
				args.Append($"\"{GetLibraryFile(processors.classpath[i])}\"");
				if(i < processors.classpath.Count - 1) {
					args.Append(";");
				}
			}

			args.Append(" ");

			//Main Class
			args.Append(GetMainClassFromJar(MainClass));

			//Args
			for (var i = 0; i < processors.args.Count; i++) {
				var carg = processors.args[i];

				carg = GetPathFromPackageName(carg);
				if (carg.StartsWith("{") && carg.EndsWith("}")) {
					carg = carg.Replace("{", "").Replace("}", "");
					carg = carg.Replace("MINECRAFT_JAR", $"\"{MINECRAFT_JAR}\"");
					carg = carg.Replace("BINPATCH", $"\"{BINPATCH_PATH_CLIENT}\"");
					
					//1.17.1
					carg = carg.Replace("INSTALLER", $"\"{INSTALLER}\"");
					carg = carg.Replace("SIDE", $"\"{SIDE}\"");
					try {
						carg = carg.Replace(carg, $"{InstallProfile.data[carg].client}");
					} catch { }
				}

				args.Append($"{carg} ");
			}

			return args.ToString();
		}

		private string GetMainClassFromJar(string MainJarPath) {
			using (var zip = ZipFile.Open(MainJarPath, ZipArchiveMode.Read)) {
				var mainfest = zip.GetEntry("META-INF/MANIFEST.MF");
				var stream = new StreamReader(mainfest.Open());
				var currentLine = String.Empty;

				while ((currentLine = stream.ReadLine()) != null) {
					if (currentLine.Contains("Main-Class:")) {
						stream.Close();
						return $"{currentLine.Replace("Main-Class:", "").Trim()} ";
					}
				}
				stream.Close();
				return "";
			}
		}

		private string GetPathFromPackageName(string mapValue) {
			if (mapValue.StartsWith("[") && mapValue.EndsWith("]")) {
				mapValue = mapValue.Replace("[", "").Replace("]", "");
				mapValue = $"\"{GetLibraryFile(mapValue)}\"";
			}
			return mapValue;
		}

		/// <summary>
		/// Get Library Full Path
		/// </summary>
		/// <param name="name">File Name</param>
		/// <returns>Library File Path</returns>
		public static string GetLibraryFile(string name) {
			if(name.StartsWith("[") && name.EndsWith("]")) {
				name = name.Replace("[", "").Replace("]", "");
			}

			return PathUtils.GetPath(AppWorkPath, ".minecraft", "libraries", GetLibraryFileName(name));
		}

		/// <summary>
		/// Get Library Path
		/// </summary>
		/// <param name="name">File Name</param>
		/// <returns>Library Path</returns>
		public static string GetLibraryFileName(string name) {
			var extinction = ".jar";
			if (name.Contains("@")) {
				extinction =  $".{name.Substring(name.LastIndexOf('@') + 1)}";
				name = name.Substring(0, name.LastIndexOf('@'));
			}

			string[] targets = name.Split(':');
			if (targets.Length < 3) return null;
			else {
				var pathBase = string.Join("\\", targets[0].Replace('.', '\\'), targets[1], targets[2], targets[1]) + '-' + targets[2];
				for(var i = 3; i < targets.Length; i++) {
					pathBase = $"{pathBase}-{targets[i]}";
				}

				pathBase = $"{pathBase}{extinction}";
				return pathBase;
			}
		}
		
		/// <summary>
		/// Download MOJANG_CLIENT_MAPPINGS
		/// </summary>
		/// <param name="mcBaseVersion">Minecraft Base Version</param>
		/// <returns>Default Save Path of Mappings File</returns>
		public static void DownloadMOJMAPS(string mcBaseVersion, string mappingFileName) {

			if (LogStatus != 0) Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][MOJMAPS] Prepare to Download");	
			var VersionClientMappingsUrl = GameVersion.Downloads.Client_Mappings.Url.Replace("https://launcher.mojang.com/", "").Replace("https://piston-meta.mojang.com/", "");

			var mappings = "";
			
			// DO YOUR STUFF: Download MOJANG Client Mappings
			
			if (LogStatus != 0) Trace.WriteLine($"[Forge-InstallProcessor.NET][Info][MOJMAPS] Save to: {mappingFileName}");

			var dire = Path.GetDirectoryName(mappingFileName);
			if (!Directory.Exists(dire)) Directory.CreateDirectory(dire);

			var sw = new StreamWriter(mappingFileName);
			sw.Write(mappings);
			sw.Close();
		}
	}

	class PathUtils {
		/// <summary>
		/// Get Path Based on Given Paths
		/// </summary>
		/// <param name="paths">Folder/File Names</param>
		/// <returns>Path</returns>
		public static string GetPath(params string[] paths) {
			return string.Join(@"\", paths);
		}

		/// <summary>
		/// Get Path Based on Given Paths
		/// </summary>
		/// <param name="paths">Folder/File Names</param>
		/// <returns>Path</returns>
		public static string GetPath(params string[][] paths) {
			string[] path = new string[paths.Length];
			for (int i = 0; i < paths.Length; i++) path[i] = PathUtils.GetPath(paths[i]);
			return PathUtils.GetPath(path);
		}
	}
}
