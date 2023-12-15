# PhotinoCustomFileServer

Shows how to embed HTML, JS, and CSS files into exe and serve them using a custom built in web server which you can access over the network as well.

Using [Photino.io ](https://www.tryphotino.io/)  allows us to create cross platform desktop applications using HTML, javascript, and CSS with C# as your backend.

# But why not just use [Photino.NET.Server](https://www.nuget.org/packages/Photino.NET.Server)?

Generally you should be using Photino.NET.Server to deal with your embedded files, but rolling your own allows for more options. One such option is being able to access your app over the network from any device with a browser like your phone.

I wrote [MixerFixerV1](https://github.com/Nicks182/MixerFixerV1) a while ago to help control my audio in Windows. It’s a WPF app using the WebView2 control and the app includes a web server. Yes, photino would have been better. While the app is running on my desktop, I can access the app over the network using the browser on my phone and get full control of the volume levels of all open apps. Sure, it’s not something you would use often and I’ve not used my phone to control the volume much at all. However, I can also open it using the browser on my desktop and this has been pretty useful.

Depending on the type of app you are building and the requirements, it may be useful (or just cool…) to be able to access the app over the network using another device with a browser.

## NOTE:

This example project will only show how to get the web server going. At the end we will look at how to make it so you can access it over the network, but this app is not properly set up for that use case and some of the javascript won’t work as it relies on the Photino Window. You will need some kind of endpoint for your page to talk to and this can be as simple as a web api endpoint.

### In the next Photino example app, I will show how to use Photino with a web server and SignalR. This will make it so you can access the app over the network and also be able to push updates to the page.

# The How
### 1. Create a new project

Start by creating a new simple Console Application. I’m using DotNet 8.0, but you can use 7.0 if you prefer. I also ticked the “Do not use top-level statements” under Additional Information, but you don’t have to.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/d14c0d88-f925-4d05-80d9-9b96570bc006)

### 2. Nuget packages

For this example we actually only need to install 2 packages, marked in red in image below. The rest will be added automatically.

Photino.NET

Microsoft.Extensions.FileProviders.Embedded

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/0754fbb0-3cef-4f16-a016-3bd9fb6d8364)

### 3. Add assets.

We need a folder to hold our web assets. For this example we will call it wwwroot.

You can add your HTML, Script, and CSS files or use the files from this repository.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/539eff33-5780-4e88-af16-d1c4c074c0eb)

### 4. Edit project file

Now we need to edit our project file. We need to specify which directory in our project we want to embed and we need to add a reference to the ‘Microsoft.AspNetCore.App’ framework which will give us access to the stuff we need to create our own web server.

Start by right clicking the project and then click on Edit Project File.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/b76d4da9-5eb2-4edf-9249-ac561c840fb4)

Next, edit the file to look like the image below.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/b3c68724-6b42-420b-b96c-f90ff6e50553)

Save the changes and you can now check under the Dependencies/Frameworks if it matches the image below.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/366e0af6-67c9-4dbf-afc2-fe4fc4a6dc82)

NOTE: We change the OuputType to WinExe otherwise we will have the console window open every time we run the app. Keeping it as ‘Exe’ can be very useful for debugging as errors will get logged to the console, but make sure it’s set to WinExe when you want to publish your app.

### 5. Custom File Server

Time for some C# code. This is based on [Photino.NET.Server](https://github.com/tryphotino/photino.NET.Server) which is why the code is very similar looking. Main difference is I’m using an Options object and a different File Provider. I also have an extra property to allow access to the page over the network. For now we will leave that option as false and we will look at that option at the end.

I’m not going to go into too much detail here. Only 2 things here are of note for our use case which is the File Provider and allowing access over the network.

**File Provider:**

We need this provider so we can serve up any embedded file to our web page. Our web page has no idea where or even how to get things like our script file or css file. The File Provider will serve all our static files including our HTML page. This allows us to embed these files into our exe instead of having them easily accessible in our app directory.

I placed the file provider code in its own method. Note the string: PhotinoCustomFileServer.wwwroot

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/f274a71a-01e0-499c-ad2a-b8ef80fcac28)

The ‘PhotinoCustomFileServer’ part of the string refers to the namespace of your project. In this case, it’s PhotinoCustomFileServer.

The ‘wwwroot’ refers to your folder containing your embedded web assets. In this case, it’s just wwwroot.

Telling our web server to use our File Provider:

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/21d1336b-0e8b-4b74-b312-1d8140f1b590)

**Local Network Access:**

I added a property to the Options class called ‘AllowLocalAccess’. If it’s set to true, you can see below that we use * to allow any IP to connect to the web server. You can limit this to certain IP ranges if you want.

By default access will be limited to localhost only. Meaning nothing will be able to connect from outside the machine the app is running on.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/a721d1cf-1369-4e20-8869-02884359fcd7)

**The code:**

Add a new class.cs file and name it what you like. For this example, I named it CustomFileServer.cs.

Add the following code to it.

Our CustomFileServer class:

```

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.Net.NetworkInformation;

namespace PhotinoCustomFileServer
{
	public class CustomFileServer
	{
    	public static WebApplication CreateStaticFileServer(CustomFileServerOptions options, out string baseUrl)
    	{
        	var builder = WebApplication
        	.CreateBuilder(new WebApplicationOptions()
        	{
            	Args = options.Args,
        	});

        	int port = options.PortStart;

        	// Try ports until available port is found
        	while (IPGlobalProperties
            	.GetIPGlobalProperties()
            	.GetActiveTcpListeners()
            	.Any(x => x.Port == port))
        	{
            	if (port > options.PortStart + options.PortRange)
            	{
                	throw new SystemException($"Couldn't find open port within range {options.PortStart} - {options.PortStart + options.PortRange}.");
            	}

            	port++;
        	}

        	baseUrl = $"http://localhost:{port}";

        	if (options.AllowLocalAccess)
        	{
            	builder.WebHost.UseUrls($"http://*:{port}");
        	}
        	else
        	{
            	builder.WebHost.UseUrls(baseUrl);
        	}

        	WebApplication app = builder.Build();


        	EmbeddedFileProvider fp = _Get_FileProvider();

        	app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fp });

        	app.UseStaticFiles(new StaticFileOptions
        	{
            	FileProvider = fp,
        	});


        	return app;
    	}

    	private static EmbeddedFileProvider _Get_FileProvider()
    	{
        	return new EmbeddedFileProvider(
            	assembly: typeof(CustomFileServer).Assembly,
            	baseNamespace: "PhotinoCustomFileServer.wwwroot");
    	}

	}

	public class CustomFileServerOptions
	{
    	public string[] Args { get; set; }
    	public int PortStart { get; set; }
    	public int PortRange { get; set; }
    	public string WebRootFolder { get; set; }

    	public bool AllowLocalAccess { get; set; }
	}
}


```

### 6. Starting the server.

In our Program.cs file in our Main method we need to use our new class to start our new server.

NOTE: We have to add [STAThread] just above our Main method.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/b1a79cca-372a-4b8d-8367-5dff10b1b268)

Add the following code so our server will start when our app loads.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/69969478-8484-425f-ac30-25548a8ca38e)


Our new class will try to find an available port for our web server, but we need to tell it in which range to look.

**PortStart:** This tells it where to start searching.

**PortRange:** This tells it how far to search.

In this example we start searching at port 8000 and it checks every port until 8100. If none of those ports are open, which should be highly unlikely, then it will throw an exception.

**WebRootFolder:** This is just the folder holding our embedded files.

**AllowLocalAcess:** If set to true, you can access the web page over the network.

**baseURL:** This will be the base url with whatever port is available and we need this to tell the Photino Window what to load.

### 7. Photino Window

Now we can create our photino window and pass in our baseURL.



For this example I’m also setting a handler for WindowCreated and WebMessageReceived.

Setting the window to be maximised during creation is bugged in version 2.5.2, but will be fixed in newer versions. For now I use this event to set the window to be maximised.

The WebMessageReceive event is to handle messages being sent from javascript to the photino window. This will not work if you access the page in your browser, but we’ll fix that in the next Photino example when I will show how to use SignalR instead.

Complete main method code with handler:

```
[STAThread]
static void Main(string[] args)
{
	CustomFileServer
	.CreateStaticFileServer(new CustomFileServerOptions
	{
    	Args = args,
    	PortStart = 8000,
    	PortRange = 100,
    	WebRootFolder = "wwwroot",
    	AllowLocalAccess = false,
	},
	out string baseURL)
	.RunAsync();


	PhotinoWindow Wind = new PhotinoWindow();
	Wind.Load(baseURL);

	#region Addition functionality
	Wind.RegisterWindowCreatedHandler(Win_WindowCreated);

	Wind.RegisterWebMessageReceivedHandler((object sender, string message) =>
	{
    	var window = (PhotinoWindow)sender;

    	// Send a message back the to JavaScript event handler.
    	window.SendWebMessage("C# reveived the following message from the browser: " + message);
	});
	#endregion Addition functionality

	Wind.WaitForClose();
}

private static void Win_WindowCreated(object? sender, EventArgs e)
{
	// Using this event seems to be the only way to get Maximized to work.
	// Trying to set it before the window is created is a bug in Photino 2.5.2 which will be fixed in newer versions.
	(sender as PhotinoWindow).SetMaximized(true);
}
```

### 8. Test it.

The app should run now if you hit F5 in Visual Studio. When the window first opens it will show an alert. Close the alert and click the button to send a message to the C# code. The C# code will just send the message straight back and you will see another alert.

**AllowLocalAcess**

Change the property, AllowLocalAcess, to true where we start our server. Start the app. You should then be able to browse to the app using a device like your phone. You will need to know the IP address of the machine the app is running on and it should normally be running on the port you specified as the PortStart. So for this example you will need to enter something like below into the browser to access the page over the network.

*http://{machineIp}:8000/*

Again, it will not work properly due to our script requiring the Photino Window. We will look at using SignalR in the next Photino Exmaple project.

### 9. Publish it.

Right click the project and select Publish.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/eb8436cd-dfd2-4143-9266-d4455b552475)

A new window will open up. Select Folder, click Next, select Folder again, click Next again. Now click the Browse button and choose where you would like to publish the files. Click Finish, then Close. You should see the image below…

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/f9683faf-c14f-4c30-bfc9-964c92675c9b)

Now click Show all settings and copy the image below.
Deployment mode = Self-contained. This means you won’t need to install .net on the system you want to run the app on as it will contain all it needs to do so on it’s own.
Target Runtime = win-x64. This needs to match the OS you wish to run the app on. I’m on Windows 10 x64 so I need to select win-x64.
Produce single file = true. So we create a single exe file.
Trim unused code is optional.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/61d3c1f7-b5b1-446b-85cc-ac7a12e2d4f5)

Click save and then click the big Publish button at the top of the screen. Once done you should see the following 4 files in your publish folder.

![image](https://github.com/Nicks182/PhotinoCustomFileServer/assets/13113785/587aff1d-6b06-4fbb-b6ea-3830c8ed824c)

Yes… it’s not really a single file. Depending on the dll, it can’t always be put into a single file. In this case I believe it’s because they are C++. The .pdb file is not needed to run the app though.
Double click the exe to run the app.
## Additional info
I often end up having a lot of small individual script and style files which I end up bundling together.

To bundle your files you can use something like: [BundlerMinifier](https://github.com/madskristensen/BundlerMinifier)

I’m currently using a more up to date version: [BundlerMinifierPlus](https://github.com/salarcode/BundlerMinifierPlus)
