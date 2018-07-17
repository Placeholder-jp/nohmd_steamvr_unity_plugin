## NoHMD SteamVRPlugin
[Origin Repo](https://github.com/ValveSoftware/steamvr_unity_plugin)

## A. ABOUT
公式のSteamVRPluginをHMDの接続なしに動作させる改造を施したプラグインです。
SteamVRの仮想HMDドライバ設定を有効にし、ViveControllerのみで立位のルームセットアップを行う機能を提供します。

## B. HOW TO USE
1. SteamVRアプリケーションを通常通りインストールします(ベータを使用するとViveTrackerのPogoPinが使用できない不具合がありました)
2. UnityEditor -> Preferences -> SteamvR Utilから "No Required HMD connection"を有効にします
![UnityPreferences](https://i.imgur.com/1M1dGyW.jpg)

## C. SCRIPTING API
- 仮想ディスプレイドライバの有効・無効切り替え  

`public static void EnableVirtualDisplayDriver(float displayFrequency = 60f)`  

`public static void DisableVirtualDisplayDriver()`  


- 任意のTrackedObjectを基準に立位モードのルームスケールを設定する  

`public static void SetWorkingStandingZeroPoseFrom(SteamVR_TrackedObject trackedObj)`