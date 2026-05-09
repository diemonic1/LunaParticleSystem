[Документация на русском](README_RU.md)

# LunaParticleSystem
Plugin for quickly editing Particle System settings in a build for Luna Playworks

<img width="388" height="928" alt="09 05 2026 - 12ч22м00с" src="https://github.com/user-attachments/assets/f67cd090-5a59-4e06-85ca-37698545f1d0" />

# Installation
Download the latest version of the plugin from [releases page](https://github.com/diemonic1/LunaParticleSystem/releases/latest)

The release includes a zip archive `LunaParticleSystemExtension.zip` with an extension for chromium-based browsers and a package `LunaParticleSystem.unitypackage`

2) - The browser extension must be installed in your browser.
   - Unpack the archive to any convenient place. The folder with the extension cannot be deleted - the browser reads the extension directly from it.
   - Open `chrome://extensions` (for Edge - `edge://extensions`).
   - In the upper right corner, turn on the Developer Mode toggle switch.
   - Click Load unpacked and select the folder where the archive was unpacked.

3) - The `LunaParticleSystem.unitypackage` package must be added to your project.
   - Open the Assets folder in Unity under Project, right-click and select Import Package - Custom Package, and then select `LunaParticleSystem.unitypackage`
   - Import assets plugins into the project.

# Usage
1) After installing the package in Unity and the extension in the browser, select any Particle System in the scene and add the `LunaParticleSystem` component to it.
Once added, ensure that the Partile System field refers to the particle system on this object.
You can add as many particle systems as you like to further customize them.

<img width="502" height="393" alt="09 05 2026 - 12ч03м27с" src="https://github.com/user-attachments/assets/dc1bd0f9-f974-401e-a4cb-f44616148718" />

2) Build the Develop Version via Tools - Unity Playworks Plugin - build Develop.

3) After successfully building the build, open it in your browser. When the build loads, a button for opening particle system settings will appear at the bottom of the page. When you click it, a side menu opens.

4) From the side menu you can:
   - Enable plugin debugging logs (written in the developer tools console - f12).
   - Change TimeScale within the build (for example, to consider particles).
   - Change any provided settings for particle systems to customize their appearance and behavior. Please note that some settings cannot be changed at all, and for some properties where you can select constant values ​​or curves as data sources, you cannot change the parameters when you select curves.

5) After setting up the particle system, it is not necessary to transfer the individual values ​​of each modified field manually. Each particle system has a copy settings button. When you click it, a JSON string with all the settings is added to the clipboard. You can paste it into the appropriate field on the `LunaParticleSystem` component, and after clicking the Apply JSON button, all parameters will be applied to the particle system automatically.
