using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor;
 
public sealed class IOSBitcodePostprocessor
{
  
   [PostProcessBuildAttribute]
   public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
       switch(target) {
           case BuildTarget.iOS:
               setupBitcode(pathToBuiltProject);
               break;
           default: break;
       }
   }
 
   private static void setupBitcode(string pathToBuiltProject) {
       var project = new PBXProject();
       var pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
       project.ReadFromFile(pbxPath);
       setupBitcodeFramework(project);
       setupBitcodeMain(project);
       project.WriteToFile(pbxPath);
   }
 
   private static void setupBitcodeFramework(PBXProject project) {
       setupBitcode(project, project.GetUnityFrameworkTargetGuid());
   }
 
   private static void setupBitcodeMain(PBXProject project) {
       setupBitcode(project, project.GetUnityMainTargetGuid());
   }
 
   private static void setupBitcode(PBXProject project, string targetGUID) {
       project.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");

        var projectGuid = project.ProjectGuid();
        string debugConfigPro = project.BuildConfigByName(projectGuid, "Debug");
        UnityEngine.Debug.Log("debugConfig:Pro" + debugConfigPro);
        string releaseConfigPro = project.BuildConfigByName(projectGuid, "Release");
        UnityEngine.Debug.Log("releaseConfig:Pro" + releaseConfigPro);
        string releaseForProfilingConfigPro = project.BuildConfigByName(projectGuid, "ReleaseForProfiling");
        UnityEngine.Debug.Log("releaseForProfilingConfigPro:" + releaseForProfilingConfigPro);
        string releaseForRunningConfigPro = project.BuildConfigByName(projectGuid, "ReleaseForRunning");
        UnityEngine.Debug.Log("releaseForRunningConfigPro:" + releaseForRunningConfigPro);

        project.SetBuildPropertyForConfig(debugConfigPro, "GCC_OPTIMIZATION_LEVEL", "0");
        project.SetBuildPropertyForConfig(releaseConfigPro, "GCC_OPTIMIZATION_LEVEL", "1");
        project.SetBuildPropertyForConfig(releaseForProfilingConfigPro, "GCC_OPTIMIZATION_LEVEL", "1");
        project.SetBuildPropertyForConfig(releaseForRunningConfigPro, "GCC_OPTIMIZATION_LEVEL", "1");
   }
}