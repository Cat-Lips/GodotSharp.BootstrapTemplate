# GodotSharp.BuildingBlocks

A template for starting a new C# Godot game development project
* Designed to be compatible with Godot master branch
* Open project.godot in Editor to import resources before running from Visual Studio
  * Included plugin will auto-name project to match parent folder then sync to C#
  * Can rename via Godot project settings and plugin will sync on save
* Includes a simple scenario-based test runner
* More addons coming...

## PREREQUISITES:
* [GodotSharp.BuildEngine](https://github.com/Cat-Lips/GodotSharp.BuildEngine) (to build master branch)
* [Vulcan SDK](https://www.lunarg.com/vulkan-sdk) (Godot runtime dependency)
* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads)
  * Workload: '.NET desktop development' or '.NET Core cross-platform development' (optional components not required)
  * Install and use [preview version]() for cs/tscn file nesting (current install still required!)
* Visual Studio Extensions (optional)
  * LineSorter (Kir_Antipov)
  * Code Cleanup On Save (Mads Christensen)
