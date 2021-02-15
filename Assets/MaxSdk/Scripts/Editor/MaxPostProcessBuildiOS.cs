//
//  MaxPostProcessBuildiOS.cs
//  AppLovin MAX Unity Plugin
//
//  Created by Santosh Bagadi on 8/5/20.
//  Copyright © 2020 AppLovin. All rights reserved.
//

#if UNITY_IOS || UNITY_IPHONE

using AppLovinMax.Scripts.IntegrationManager.Editor;
#if UNITY_2019_3_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace AppLovinMax.Scripts.Editor
{
    [Serializable]
    public class SkAdNetworkData
    {
        [SerializeField] public string[] SkAdNetworkIds;
    }

    public class MaxPostProcessBuildiOS
    {
        private const string TargetUnityiPhonePodfileLine = "target 'Unity-iPhone' do";
        
        private static readonly List<string> AtsRequiringNetworks = new List<string>
        {
            "AdColony",
            "ByteDance",
            "Fyber",
            "Google",
            "GoogleAdManager",
            "HyprMX",
            "InMobi",
            "IronSource",
            "Smaato"
        };

        private static List<string> DynamicLibraryPathsToEmbed
        {
            get
            {
                var dynamicLibraryPathsToEmbed = new List<string>(2);
                dynamicLibraryPathsToEmbed.Add(Path.Combine("Pods/", "HyprMX/HyprMX.xcframework"));
                if (ShouldEmbedSnapSdk())
                {
                    dynamicLibraryPathsToEmbed.Add(Path.Combine("Pods/", "SAKSDK/SAKSDK.framework"));
                }

                return dynamicLibraryPathsToEmbed;
            }
        }

        [PostProcessBuildAttribute(int.MaxValue)]
        public static void MaxPostProcessEmbedDynamicLibraries(BuildTarget buildTarget, string path)
        {
            var dynamicLibraryPathsPresentInProject = DynamicLibraryPathsToEmbed.Where(dynamicLibraryPath => Directory.Exists(Path.Combine(path, dynamicLibraryPath))).ToList();
            if (dynamicLibraryPathsPresentInProject.Count <= 0) return;

            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
            var targetGuid = project.GetUnityMainTargetGuid();
            var containsUnityiPhoneTargetInPodfile = ContainsUnityiPhoneTargetInPodfile(path);
            // Embed framework if it is .xcframework or is .framework and the podfile does not contain target `Unity-iPhone`.
            foreach (var dynamicLibraryPath in dynamicLibraryPathsPresentInProject)
            {
                if (dynamicLibraryPath.EndsWith(".framework") && containsUnityiPhoneTargetInPodfile) continue;
                
                var fileGuid = project.AddFile(dynamicLibraryPath, dynamicLibraryPath);
                project.AddFileToEmbedFrameworks(targetGuid, fileGuid);
            }
#else
            var targetGuid = project.TargetGuidByName("Unity-iPhone");
            string runpathSearchPaths;
#if UNITY_2018_2_OR_NEWER
            runpathSearchPaths = project.GetBuildPropertyForAnyConfig(targetGuid, "LD_RUNPATH_SEARCH_PATHS");
#else
            runpathSearchPaths = "$(inherited)";          
#endif
            runpathSearchPaths += string.IsNullOrEmpty(runpathSearchPaths) ? "" : " ";

            // Check if runtime search paths already contains the required search paths for dynamic libraries.
            if (runpathSearchPaths.Contains("@executable_path/Frameworks")) return;

            runpathSearchPaths += "@executable_path/Frameworks";
            project.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", runpathSearchPaths);
#endif
            if (ShouldEmbedSnapSdk())
            {
                // Needed to build successfully on Xcode 12+, as Snap was build with latest Xcode but not as an xcframework
                project.AddBuildProperty(targetGuid, "VALIDATE_WORKSPACE", "YES");
            }

            project.WriteToFile(projectPath);
        }

        [PostProcessBuildAttribute(int.MaxValue)]
        public static void MaxPostProcessPlist(BuildTarget buildTarget, string path)
        {
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            
#if UNITY_2018_2_OR_NEWER
            EnableVerboseLoggingIfNeeded(plist);
#endif
//            EnableConsentFlowIfNeeded(plist);
            AddSkAdNetworksInfoIfNeeded(plist);
            UpdateAppTransportSecuritySettingsIfNeeded(plist);

            plist.WriteToFile(plistPath);
        }
        
#if UNITY_2018_2_OR_NEWER
        private static void EnableVerboseLoggingIfNeeded(PlistDocument plist)
        {
            if (!EditorPrefs.HasKey(MaxSdkLogger.KeyVerboseLoggingEnabled)) return;

            var enabled = EditorPrefs.GetBool(MaxSdkLogger.KeyVerboseLoggingEnabled);
            const string AppLovinVerboseLoggingOnKey = "AppLovinVerboseLoggingOn";
            if (enabled)
            {
                plist.root.SetBoolean(AppLovinVerboseLoggingOnKey, enabled);
            }
            else
            {
                plist.root.values.Remove(AppLovinVerboseLoggingOnKey);
            }
        }
#endif

//        private static void EnableConsentFlowIfNeeded(PlistDocument plist)
//        {
//            // Check if consent flow is enabled. No need to update info.plist if consent flow is disabled.
//            var consentFlowEnabled = AppLovinSettings.Instance.ConsentFlowEnabled;
//            if (!consentFlowEnabled) return;
//
//            var userTrackingUsageDescription = AppLovinSettings.Instance.UserTrackingUsageDescription;
//            var termsOfServiceUrl = AppLovinSettings.Instance.ConsentFlowTermsOfServiceUrl;
//            var privacyPolicyUrl = AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl;
//            if (string.IsNullOrEmpty(userTrackingUsageDescription) || string.IsNullOrEmpty(termsOfServiceUrl) || string.IsNullOrEmpty(privacyPolicyUrl))
//            {
//                AppLovinIntegrationManager.ShowBuildFailureDialog("You cannot use the AppLovin SDK's consent flow without defining a Terms of Service URL, a Privacy Policy URL and the `User Tracking Usage Description` in the AppLovin Integration Manager. \n\n" +
//                                                                  "All 3 values must be included to enable the SDK's consent flow.");
//
//                // No need to update the info.plist here. Default consent flow state will be determined on the SDK side.
//                return;
//            }
//
//            var consentFlowInfoRoot = plist.root.CreateDict("AppLovinConsentFlowInfo");
//            consentFlowInfoRoot.SetBoolean("AppLovinConsentFlowEnabled", consentFlowEnabled);
//            consentFlowInfoRoot.SetString("AppLovinConsentFlowTermsOfService", termsOfServiceUrl);
//            consentFlowInfoRoot.SetString("AppLovinConsentFlowPrivacyPolicy", privacyPolicyUrl);
//
//            plist.root.SetString("NSUserTrackingUsageDescription", userTrackingUsageDescription);
//        }

        private static void AddSkAdNetworksInfoIfNeeded(PlistDocument plist)
        {
            var skAdNetworkData = GetSkAdNetworkData();
            var skAdNetworkIds = skAdNetworkData.SkAdNetworkIds;
            // Check if we have a valid list of SKAdNetworkIds that need to be added.
            if (skAdNetworkIds == null || skAdNetworkIds.Length < 1) return;

            //
            // Add the SKAdNetworkItems to the plist. It should look like following:
            //
            //    <key>SKAdNetworkItems</key>
            //    <array>
            //        <dict>
            //            <key>SKAdNetworkIdentifier</key>
            //            <string>ABC123XYZ.skadnetwork</string>
            //        </dict>
            //        <dict>
            //            <key>SKAdNetworkIdentifier</key>
            //            <string>123QWE456.skadnetwork</string>
            //        </dict>
            //        <dict>
            //            <key>SKAdNetworkIdentifier</key>
            //            <string>987XYZ123.skadnetwork</string>
            //        </dict>
            //    </array>
            //
            PlistElement skAdNetworkItems;
            plist.root.values.TryGetValue("SKAdNetworkItems", out skAdNetworkItems);
            var existingSkAdNetworkIds = new HashSet<string>();
            // Check if SKAdNetworkItems array is already in the Plist document and collect all the IDs that are already present.
            if (skAdNetworkItems != null && skAdNetworkItems.GetType() == typeof(PlistElementArray))
            {
                var plistElementDictionaries = skAdNetworkItems.AsArray().values.Where(plistElement => plistElement.GetType() == typeof(PlistElementDict));
                foreach (var plistElement in plistElementDictionaries)
                {
                    PlistElement existingId;
                    plistElement.AsDict().values.TryGetValue("SKAdNetworkIdentifier", out existingId);
                    if (existingId == null || existingId.GetType() != typeof(PlistElementString) || string.IsNullOrEmpty(existingId.AsString())) continue;

                    existingSkAdNetworkIds.Add(existingId.AsString());
                }
            }
            // Else, create an array of SKAdNetworkItems into which we will add our IDs.
            else
            {
                skAdNetworkItems = plist.root.CreateArray("SKAdNetworkItems");
            }

            foreach (var skAdNetworkId in skAdNetworkIds)
            {
                // Skip adding IDs that are already in the array.
                if (existingSkAdNetworkIds.Contains(skAdNetworkId)) continue;

                var skAdNetworkItemDict = skAdNetworkItems.AsArray().AddDict();
                skAdNetworkItemDict.SetString("SKAdNetworkIdentifier", skAdNetworkId);
            }
        }

        private static SkAdNetworkData GetSkAdNetworkData()
        {
            var uriBuilder = new UriBuilder("https://dash.applovin.com/docs/v1/unity_integration_manager/sk_ad_networks_info");

            // Get the list of installed ad networks to be passed up
            var pluginParentDir = AppLovinIntegrationManager.MediationSpecificPluginParentDirectory;
            var maxMediationDirectory = Path.Combine(pluginParentDir, "MaxSdk/Mediation/");
            if (Directory.Exists(maxMediationDirectory))
            {
                var mediationNetworkDirectories = Directory.GetDirectories(maxMediationDirectory);
                var installedNetworks = mediationNetworkDirectories.Select(Path.GetFileName).ToArray();
                var adNetworks = string.Join(",", installedNetworks);
                if (!string.IsNullOrEmpty(adNetworks))
                {
                    uriBuilder.Query += string.Format("adnetworks={0}", adNetworks);
                }
            }

            var unityWebRequest = UnityWebRequest.Get(uriBuilder.ToString());

#if UNITY_2017_2_OR_NEWER
            var operation = unityWebRequest.SendWebRequest();
#else
            var operation = unityWebRequest.Send();
#endif
            // Wait for the download to complete or the request to timeout.
            while (!operation.isDone) { }


#if UNITY_2020_1_OR_NEWER
            if (unityWebRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
#else
            if (unityWebRequest.isError)
#endif
            {
                MaxSdkLogger.UserError("Failed to retrieve SKAdNetwork IDs with error: " + unityWebRequest.error);
                return new SkAdNetworkData();
            }

            try
            {
                return JsonUtility.FromJson<SkAdNetworkData>(unityWebRequest.downloadHandler.text);
            }
            catch (Exception exception)
            {
                MaxSdkLogger.UserError("Failed to parse data '" + unityWebRequest.downloadHandler.text + "' with exception: " + exception);
                return new SkAdNetworkData();
            }
        }

        private static void UpdateAppTransportSecuritySettingsIfNeeded(PlistDocument plist)
        {
            var pluginParentDir = AppLovinIntegrationManager.MediationSpecificPluginParentDirectory;
            var mediationDir = Path.Combine(pluginParentDir, "MaxSdk/Mediation/");
            var projectHasAtsRequiringNetworks = AtsRequiringNetworks.Any(atsRequiringNetwork => Directory.Exists(Path.Combine(mediationDir, atsRequiringNetwork)));
            if (!projectHasAtsRequiringNetworks) return;

            var root = plist.root.values;
            PlistElement atsRoot;
            root.TryGetValue("NSAppTransportSecurity", out atsRoot);

            if (atsRoot == null || atsRoot.GetType() != typeof(PlistElementDict))
            {
                // Add the missing App Transport Security settings for publishers if needed. 
                MaxSdkLogger.UserDebug("Adding App Transport Security settings...");
                atsRoot = plist.root.CreateDict("NSAppTransportSecurity");
                atsRoot.AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
            }

            var atsRootDict = atsRoot.AsDict().values;
            // Check if both NSAllowsArbitraryLoads and NSAllowsArbitraryLoadsInWebContent are present and remove NSAllowsArbitraryLoadsInWebContent if both are present.
            if (atsRootDict.ContainsKey("NSAllowsArbitraryLoads") && atsRootDict.ContainsKey("NSAllowsArbitraryLoadsInWebContent"))
            {
                MaxSdkLogger.UserDebug("Removing NSAllowsArbitraryLoadsInWebContent");
                atsRootDict.Remove("NSAllowsArbitraryLoadsInWebContent");
            }
        }

        private static bool ShouldEmbedSnapSdk()
        {
            var pluginParentDir = AppLovinIntegrationManager.MediationSpecificPluginParentDirectory;
            var snapDependencyPath = Path.Combine(pluginParentDir, "MaxSdk/Mediation/Snap/Editor/Dependencies.xml");
            if (!File.Exists(snapDependencyPath)) return false;

            // Return true for UNITY_2019_3_OR_NEWER because app will crash on launch unless embedded.
#if UNITY_2019_3_OR_NEWER
            return true;
#else
            var currentVersion = AppLovinIntegrationManager.GetCurrentVersions(snapDependencyPath);
            var iosVersionComparison = MaxSdkUtils.CompareVersions(currentVersion.Ios, "1.0.7.2");
            return iosVersionComparison != MaxSdkUtils.VersionComparisonResult.Lesser;
#endif
        }

#if UNITY_2019_3_OR_NEWER
        private static bool ContainsUnityiPhoneTargetInPodfile(string buildPath)
        {
            var podfilePath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfilePath)) return false;

            var lines = File.ReadAllLines(podfilePath);
            return lines.Any(line => line.Contains(TargetUnityiPhonePodfileLine));
        }
#endif
    }
}

#endif
