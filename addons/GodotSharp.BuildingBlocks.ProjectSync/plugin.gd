@tool
extends EditorPlugin


func _enter_tree():
    print('Activating Plugin: GodotSharp.BuildingBlocks.ProjectSync')


func _ready():
    save_external_data()


func _exit_tree():
    print('Deactivating Plugin: GodotSharp.BuildingBlocks.ProjectSync')


func save_external_data():
    var project_name = ProjectSettings.get_setting('application/config/name')
    var project_path = ProjectSettings.globalize_path('res://').trim_suffix('/')

    if project_name == '':
        project_name = project_path.get_file()
        print("[ProjectSync] Setting project name to '%s'" % project_name)
        ProjectSettings.set_setting('application/config/name', project_name)
        ProjectSettings.save()

    var user_data_dir = ProjectSettings.globalize_path('user://').trim_suffix('/')
    var plugin_path = ProjectSettings.globalize_path(get_script().get_path().get_base_dir())
    var godot_exe = OS.get_executable_path()

    var output = []
    var project_sync = plugin_path.plus_file('Sync-Project.ps1')
    var args = [
        '-WindowStyle', 'Hidden', 
        '-ExecutionPolicy', 'Bypass', 
        project_sync.replace(' ', '` '),
        project_name.replace(' ', '` '),
        project_path.replace(' ', '` '),
        user_data_dir.replace(' ', '` '),
        godot_exe.replace(' ', '` ')]
    OS.execute('powershell', args, output, true)
    for line in output:
        print(line)
