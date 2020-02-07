# Forge-InstallProcessor.NET
C# Methods to Install Minecraft Forge 1.13+

## Requirement
.Net Framework 4.5 or Higher Due to using the `System.IO.Compression.FileSystem`.  
You can modify the `getMainClass();` Method in `Installer.Processor` in order to running in .Net Framework 4.0(Yes!) or Lower(Maybe?).

## Who need this?
If you want to Install Forge without showing the Forge Installer GUI, you can use this method to install Forge.  
The Minecraft Launcher using this method: [BakaXL - Minecraft Launcher](http://www.BakaXL.com/) 
| Minecraft Version | Forge Version | Supported?
| ------ | ------ | ------ |
| Minecraft 1.12 or Below | Forge 1.12.2 - 14.23.5.2847 or Below | :x:
| Minecraft 1.13.2 or Higher | Forge 1.13.2 - 25.0.160 or Higher | :heavy_check_mark:

## Usage
1. Download Forge Installer from Forge Offical Website. ([Here](https://files.minecraftforge.net/))
2. Extract Installer.
3. Use whatevere the method to Convert `"install_profile.json"` to `Installer.Forge.InstallProfile` Object (e.g LitJson).
    ```csharp
    //Create InstallProfile Object
    var installProfile= new Forge.InstallProfile();
    using (var sr = new StreamReader("install_profile.json")) {
			try {
			    //Use LitJson to Convert Json String to Object.
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
4. Download all the nesseary libraries inside `installProfile.libraries`.   
&emsp;&emsp;4.1 Notice that there are two libraries already included in Forge Install Jar (e.g: Forge 1.15.2-31.1.0)  
&emsp;&emsp;&emsp;&emsp;`"maven/net/minecraftforge/forge/1.15.2-31.1.0/forge-1.15.2-31.1.0.jar"`  
&emsp;&emsp;&emsp;&emsp;`"maven/net/minecraftforge/forge/1.15.2-31.1.0/forge-1.15.2-31.1.0-universal.jar"`  
&emsp;&emsp;&emsp;&emsp;Copy these two file to `".minecraft/libraries/net/minecraftforge/forge/1.15.2-31.0.13"` Folder.
5. Create Installer.Processor Instance.
    ```csharp
      var processor = new Installer.Processor();
    ```
5. Initialize Installer.Processor.
    ```csharp
      if(processor.Init(Installer.Forge.InstallProfile, IProgress<Installer.InstallTask>)) {
          //Begin Installation.
          processor.Begin();
      } else { 
          //Initialize Failed.
          //Do something here...
      }
    ```
6. Done!

## Modify
`IProgress<Installer.InstallTask>` is not necessary, it can let processor report current stage.  
You can remove it from the code if you want.

## See also
My friends are also doing some greate job on Installing Forge 1.13+. Please also have a look.
* [bangbang93 - Forge Install Bootstrapper](https://github.com/bangbang93/forge-install-bootstrapper) (Java)
* [xfl03 - Forge Installer Headless](https://github.com/xfl03/ForgeInstallerHeadless) (Java)
* [Nsiso Launcher](https://github.com/Nsiso/NsisoLauncher/tree/dev/src/NsisoLauncherCore/Util/Installer/Forge) (.Net C#)
