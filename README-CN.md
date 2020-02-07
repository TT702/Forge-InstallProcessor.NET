# Forge-InstallProcessor.NET
一个使用C#语言安装Minecraft Forge 1.13或更高级版本的类。

[English Version](https://github.com/TT702/Forge-InstallProcessor.NET/blob/master/README.md)

## 需求
首先你需要使用 .Net Framework 4.5或更高级版本，因为使用了`System.IO.Compression.FileSystem`来处理压缩文件。  
当然你也可以改一改位于`Installer.Processor` 的 `GetMainClassFromJar();` 这个方法。  
即使用自己的解压缩方式进行解压缩，然后读取JAR里的MANIFEST.MF来获取MainClass信息。

## 在何种情况下要使用这个类
如果你是启动器作者，并且不想在启动器安装Forge时显示界面，也不想在安装Forge前下载一个注入器来做这些事。
你可以试试用这个类来处理Forge的安装。目前 [BakaXL 启动器](http://www.BakaXL.com/) 正在使用这个方式进行1.13+的Forge安装。
| Minecraft 版本 | Forge 版本 | 支持吗?
| ------ | ------ | ------ |
| Minecraft 1.12 或更低版本 | Forge 1.12.2 - 14.23.5.2847 或更低版本 | :x:
| Minecraft 1.13.2 或更高版本 | Forge 1.13.2 - 25.0.160 或更高版本 | :heavy_check_mark:

## 使用方式
1. 首先你需要下载好Minecraft原版核心，并且将其存放于 `<启动器的位置>/.minecraft/versions/<Minecraft 版本>`
2. 从Forge官方或者其他地方下载Forge的Installer ([Forge官方](https://files.minecraftforge.net/))
3. 写个代码或者啥的解压Installer.jar
4. 用任意方式把解压出来的 `"install_profile.json"` 转换为 `Installer.Forge.InstallProfile` 对象 (此代码里使用的LitJson，要修改请随意。)
    ```csharp
    //创建InstallProfile对象
    var installProfile= new Forge.InstallProfile();
    using (var sr = new StreamReader("install_profile.json")) {
			try {
			    //使用LitJson将Json字符串转换为对象
			    installProfile = JsonMapper.ToObject<Forge.InstallProfile>(sr.ReadToEnd());
          sr.Close();
			} catch (Exception ex) {
			    Trace.WriteLine("[Error]Failed while Read" + ex);
			    throw ex;
			}
			if (installProfile == null) {
			    Trace.WriteLine("[Error]Failed.");
		    	sr.Close();
      }
    }
    ```
5. 从 `installProfile.libraries` 对象里下载所有安装时需要的Libraries。   
&emsp;&emsp;5.1 有两个Libraries在Json里是没有下载地址的，因为他们已经被包括在了Installer里了，需要你手动解压并复制到对应的位置。 (例如你的版本是Forge 1.15.2-31.1.0)  
&emsp;&emsp;&emsp;&emsp;`"maven/net/minecraftforge/forge/1.15.2-31.1.0/forge-1.15.2-31.1.0.jar"`  
&emsp;&emsp;&emsp;&emsp;`"maven/net/minecraftforge/forge/1.15.2-31.1.0/forge-1.15.2-31.1.0-universal.jar"`  
&emsp;&emsp;&emsp;&emsp;那你就要把Installer里解压出来的上述两个文件复制到 `".minecraft/libraries/net/minecraftforge/forge/1.15.2-31.0.13"` 文件夹。
6. 创建 Installer.Processor 实例。
    ```csharp
      var processor = new Installer.Processor();
      
      //设置日志输出等级
      processor.LogStatus = Installer.Processor.LogLevel.Verbose;
    ```
7. 初始化 Installer.Processor
    ```csharp
      if(processor.Init(Installer.Forge.InstallProfile, "Java.exe的路径")) {
          //开始初始化
          processor.Begin();
      } else { 
          //初始化失败，Forge Installer版本不兼容
          //干点别的事…
      }
    ```
8. 完成！

## 日志输出等级
Processor支持三种不同的日志输出等级。
| 输出 | 描述
| ------ | ------ |
| None | 咩都无，又穷又样衰。
| InstallOnly | 安装的过程和Forge Processor的输出。
| Verbose | 输出所有东西，包括合并的Java参数，啰里吧嗦的。

## 一些有的没的
`IProgress<Installer.InstallTask>` 可以让你通过IProgress来传递当前安装器的进度和状态。
由于把它单独提出来也有点麻烦，所以我注释掉了。  
你可以把它删了。  

## 一些使用上的提示
Forge的安装路径将会是 `<启动器的位置>/.minecraft/versions/<Forge版本>`.
以及由于代码是直接从BakaXL源代码里复制出来的，所以你可能需要手动修改一下命名空间。

## 另请参阅
位于 Minecraft 启动器交流群里的各位好朋友们都各自挑战了一把 Forge 1.13+ 的自动安装，以下是他们的成果。  
不妨也试试他们的安装器，请大家自行适合自己项目的解决方案。
* [bangbang93 - Forge Install Bootstrapper](https://github.com/bangbang93/forge-install-bootstrapper) (Java)
* [xfl03 - Forge Installer Headless](https://github.com/xfl03/ForgeInstallerHeadless) (Java)
* [Nsiso Launcher](https://github.com/Nsiso/NsisoLauncher/tree/dev/src/NsisoLauncherCore/Util/Installer/Forge) (.Net C#)
