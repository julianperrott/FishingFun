
# Fishing Fun - A World of Warcraft Bot

In World of Warcraft, Fishing is a time consuming task which is simple and so lends itself to automation very well. There are many fishing bots out there. This article describes the bot that I wrote for fun and the problems I solved to make it work.

### Why Fish ?

Fishing is a way to catch fish which can be used to cook food, which is used to heal or increase the stats of your character. You can also gain achievements through fishing. 

The mechanics of fishing involve casting your line into in-land or sea waters and then waiting up to 30 seconds for a bite, then clicking to loot within a couple of seconds to catch the fish.

## Video of the bot in action

https://www.youtube.com/watch?v=T6reHXxA5f0

[![Fishing Fun YouTube](https://img.youtube.com/vi/T6reHXxA5f0/0.jpg)](https://www.youtube.com/watch?v=T6reHXxA5f0)

# Getting it working

## 1. Download this repository

Put the contents of the repo into a folder. e.g "C:\FishingFun". I am going to refer to this folder from now on, so just substitute your own folder path.

## 2. Install Visual Studio

You will need to install Visual Studio, get it here: https://visualstudio.microsoft.com/vs/ use Community 2019 or later.
- Under 'Desktop & mobile' - Select .Net desktop development.
- Here is a video which shows installation (note you don't need python) https://www.youtube.com/watch?v=1uBESL2S8Ik&ab_channel=JonJensen

Requires: Dot net framework 4.7 https://dotnet.microsoft.com/download/dotnet-framework, this should automatically be installed with visual studio.

## 3. Double click on the solution file to open it: "C:\FishingFun\source\FishingFun.sln", or open it from visual studio.

Once loaded click the Start button, this should build and run the project FishingFun.UI. Alternatively use F5 or menu 'Debug -> Start Debugging'.

Once it has built you can run it without visual studio by navigating with File Explorer to folder C:\FishingFun\Source\bin\Debug and run by double clicking on Chrome.exe

If you want run a version without a GUI you need to set the startup project in visual studio to FishingFun.Console by right clicking on the project in the Solution Explorer and choosing 'Set as startup project', then build (F6). You should now have C:\FishingFun\Source\bin\Debug\powershell.exe which you can run outside of visual studio.

## 4. Bot Running Instructions

* Enable 'Autoloot' (Esc, Interface Options, Controls, Check Autoloot)
* Turn off Click to move (Esc, Interface Options, Mouse, Uncheck Click to move)
* Make sure 'Right Click' loots.
* Put the cast fishing Button on poistion 4 of your Action Bar. I.e. when 4 is clicked fishing will cast.
* Zoom in completely so your character is not visible.
* Make sure the fishing float is in the middle of the screen.
* Fish at close to ground level as you can, not off docks or other high places.
* It requires that WOW is running Full screen (not windowed) on the primary screen. It needs the wow screen to be on top. So it probably won't work well if you only have one screen. If you need it windowed then I suggest you look at the code in this fork: https://github.com/petrvecera/FishingFun
* Sometimes you may need to adjust the colour parameters for the bobber finder, such as during evening time or if there is a lot of red in the landscape.

----

## All about the bot

### To catch fish, what would a bot have to do ?

The bot needs to transition through the following states:

* Casting.
* Watching the bobber for a bite. If a bite is seen then move to Looting. If the bobber is not seen for a few seconds or 30 seconds elapses then move back to the casting state.
* Looting.

### What problems are there to solve ?

The main problems are: 

* Finding the coordinates of the bobber on the screen.
* Then determining when a bite has taken place.

#### Problem 1: Finding the bobber

The bobber is tiny on the screen, we need to make it easier to find. 

![Screen Zoomed Out](/post/img/fishingfun_zoomedout.jpg)


Changing the character view to fully zoomed in means that the bobber is bigger and there is less clutter on the screen. 

To further simplify finding the bobber, it must appear in the middle half of the screen as viewed by the character. Indicated by the red area in the image below.

![Screen Zoomed In](/post/img/FishingFun_ZoomedIn.jpg)

The bobber is pretty easy for us to spot now, but a computer needs a simple way to determine where the bobber is. We could train an AI to find the float, but that seems like an over complicated solution. Perhaps we can use the red colour of the bobber to locate it ?

If we find all the red pixels in middle half of the screen, then find the pixel with most red pixels around it then we should have our bobber location !

We can get a bitmap of the screen as below:
<pre class="prettyprint">
public static Bitmap GetBitmap()
{
    var bmpScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);
    var graphics = Graphics.FromImage(bmpScreen);
    graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.Width / 4, Screen.PrimaryScreen.Bounds.Height / 4, 0, 0, bmpScreen.Size);
    graphics.Dispose();
    return bmpScreen;
}
</pre>

#### Problem 2: Determining when a bite has taken place.

When a bite occurs the bobber moves down a few pixels. If we track the position of the bobber while fishing, we can see an obvious change in the Y position when the bite happens.

### Determining the location of the red feather on the bobber

Due to the different times of day and environments in the game, the red bobber feather changes its shade of red, it also has a range of red shades within it. We need to classify all these colours as being red.

![Fishing Bobbers](/post/img/fishingfun_bobbers.png)

The pixels we are looking for are going have an RGB value with a high Red value compared to the Green and Blue. In the colour cube below we are looking for the colours in the back left.

![Colour Cube](/post/img/finshingfun_cube.png)

This is the algorithm I have created to determine redness:

* Red is greater that Blue and Green by a chosen percentage e.g. 200%.
* Blue and Green are reasonably close together.

<pre class="prettyprint" >
public double ColourMultiplier { get; set; } = 0.5;
public double ColourClosenessMultiplier { get; set; } = 2.0;

public bool IsMatch(byte red, byte green, byte blue)
{
    return isBigger(red, green) && isBigger(red, blue) && areClose(blue, green);
}

private bool isBigger(byte red, byte other)
{
    return (red * ColourMultiplier) > other;
}

private bool areClose(byte color1, byte color2)
{
    var max = Math.Max(color1, color2);
    var min = Math.Min(color1, color2);

    return min * ColourClosenessMultiplier > max - 20;
}
</pre>

In the animation below which shows the Red value changing from 0 to 255 within a 2D square of all possible Blue and Green values, the algorithm matches the red colours within the white boundary. These are all the possible colours which it considers as being in the red feather.

![Red Match Animation](/post/img/fishingfun_red.png)

----

### The User Interface

The WPF user interface I created allows the user to see what the bot sees and how it is doing finding the bobber. This 
helps to determine how well it is working.

#### Main Screen

On the left of the UI there is an screenshot which shows the part of the screen being monitored, the bobber position is indicated by a recticle, the recognised red pixels are shown in pure red colour.

![Screenshot1](/post/img/FishingFun_Screenshot1.jpg)



In the top right the amplitude of the bobber is shown in an animated graph ([lvcharts.net](https://lvcharts.net/)). It moves up and down a few pixels during fishing. When the bite occurs it drops 7 or more pixels.

![Screenshot Looting](https://raw.githubusercontent.com/julianperrott/FishingFun/master/post/img/Screenshot2.png "Fishing Fun - Looting")

#### Colour Configuration Screen

A second configuration screen allows the investigation of different settings for the 'Red' pixel detection.

![Screenshot3](/post/img/FishingFun_Screenshot3.jpg)


### Console Version

A console version is also available if the UI is not needed. It exposes the log so that some feedback on the bot performance is given to the user.

![Screenshot Console](/post/img/FishingFun_Console.png)







