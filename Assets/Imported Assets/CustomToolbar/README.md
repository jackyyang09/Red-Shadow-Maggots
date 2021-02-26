# CustomToolbar
![Unity 2019.4+](https://img.shields.io/badge/unity-unity%202019.4%2B-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)

based on this [marijnz unity-toolbar-extender](https://github.com/marijnz/unity-toolbar-extender). 

![image](https://user-images.githubusercontent.com/16706911/100000419-cff31e00-2dd6-11eb-9a4b-8379e3a7cc50.jpg)



### Why you should use the CustomToolbar?
This custom tool helps you to test and develop your game easily

## Installation 
You can also install via git url by adding this entry in your **manifest.json**

```"com.smkplus.custom-toolbar": "https://github.com/smkplus/CustomToolbar.git#master",```

## Installation through Unity-Package-Manager (2019.2+) 
 * MenuItem - Window - Package Manager
 * Add package from git url
 * paste https://github.com/smkplus/CustomToolbar.git#master

## Sample scenes to test  
You can import sample scenes from package manager. 

![image](Images~/Package-Manager.png)
____________
Scene selection dropdown to open scene in editor. Scenes in build have unity icon while selected and appear above splitter in list

![image](Images~/SceneSelect.jpg)
____________

when you want to clear all playerprefs you have to follow 3 step:

![image](https://user-images.githubusercontent.com/16706911/68548191-52dd4c80-03ff-11ea-85b6-e9899ab04c34.jpg)

but you can easily Clear them by clicking on this button:

![image](Images~/btnClearPrefs.jpg)
____________

another button relevant to saving is this button that prevents saving during the gameplay. because sometimes you have to Clear All playerprefs after each test so you can enable this toggle:

Enable Playerprefs:

![image](Images~/btnDisablePrefs.jpg)

Disable Playerprefs:

![image](Images~/btnDisablePrefsInactive.jpg)
____________

you can restart the active scene by this button:

![image](Images~/btnRestartScene.jpg)
____________

suppose you want to test your game so you should start game from scene 1(Menu):

![image](https://user-images.githubusercontent.com/16706911/68548295-8371b600-0400-11ea-8737-a9da3d555df0.png)

you have to find scene 1 (Menu):

![image](https://user-images.githubusercontent.com/16706911/68548309-c2a00700-0400-11ea-9740-128368bd801a.png)

then you should start the game:

![image](https://user-images.githubusercontent.com/16706911/100723264-cd945380-33d6-11eb-9611-b1fe470dbd0b.png)

this button is shortcut to start the game from scene 1:

![image](Images~/btnFirstScene.jpg)
____________

I usually test my games by changing timescale.

![image](Images~/timescale.jpg)
____________

Also it usefull to test your game with different framerates, to be sure that it is framerate-independent.

![image](Images~/FPS.jpg)
____________

Button to recompile scripts. Usefull when you working on splitting code into .asmdef

![image](Images~/btnRecompile.jpg)
____________

Force reserialize selected(in project window) assets. What it does - https://docs.unity3d.com/ScriptReference/AssetDatabase.ForceReserializeAssets.html

![image](Images~/btnReserializeSelected.jpg)
____________

Force reserialize all assets. Same as previous, but for all assets and takes some time. Use this after adding new asset or updating unity version in order to not spam git history with unwanted changes.

![image](Images~/btnReserializeAll.jpg)
____________
  
You can customize the toolbar on Project Setting

![Images~/ProjectSetting-CustomToolbar.png](Images~/ProjectSetting-CustomToolbar.png)

_____

## How to Contribute

Development directory:

1. Create a project through Unity Hub.
2. clone repository at `Package` folder at root of project.
3. edit codes with your IDE.

![image](https://user-images.githubusercontent.com/51351749/100615833-8452ee00-335b-11eb-9b29-463810651ec7.png)

![image](https://user-images.githubusercontent.com/51351749/100615844-8ae16580-335b-11eb-8a1e-4600bfe75454.png)

![image](https://user-images.githubusercontent.com/51351749/100615856-8e74ec80-335b-11eb-97c5-a6ff2fb1dd8f.png)




