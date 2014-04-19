using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class QuickBloxCreator
{
	
	
	public static BuildTarget CurrentDevice
	{
		get; set;
	}
	
    [MenuItem("CloudStorage/Create AssetBundle",false,1)]
    static void Execute()
    {
		
		
		QuickBloxCreator.CurrentDevice = Settings._UserSettings.Target;
		
		switch(QuickBloxCreator.CurrentDevice)
		{
			case BuildTarget.iPhone:
			{
				break;	
			}
			case BuildTarget.StandaloneOSXIntel:
			{
				break;	
			}
			case BuildTarget.Android:
			{
				break;	
			}
			case BuildTarget.StandaloneWindows:
			{
				break;	
			}
			case BuildTarget.StandaloneLinuxUniversal:
			{
				break;	
			}
			case BuildTarget.PS3:
			{
				break;	
			}
			case BuildTarget.Wii:
			{
				break;	
			}
			case BuildTarget.XBOX360:
			{
				break;	
			}
		default:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.iPhone;
			break;
			}
		}
		
		
		
		
        foreach (Object selectedObject in Selection.GetFiltered(typeof (Object), SelectionMode.DeepAssets))
        {
            if (!(selectedObject is GameObject)) continue;

            GameObject gameObject = (GameObject)selectedObject;
            string name = gameObject.name.ToLower();

            if (!Directory.Exists(AssetbundlePath))
                Directory.CreateDirectory(AssetbundlePath);

            string[] existingAssetbundles = Directory.GetFiles(AssetbundlePath);
            foreach (string bundle in existingAssetbundles)
            {
                if (bundle.EndsWith(".assetbundle") && bundle.Contains("/AssetBundles/" + name))
                    File.Delete(bundle);
			}
     
			GameObject tempObject = GameObject.Instantiate(gameObject) as GameObject;
            Object currentPrefab = GetPrefab(tempObject, name + "Temp");
            string path = AssetbundlePath + name + ".assetbundle";
			if(!BuildPipeline.BuildAssetBundle(currentPrefab, null, path, BuildAssetBundleOptions.CollectDependencies,CurrentDevice))
			{
				Debug.LogError("You can not build an asset bundle for the current target.");
			}          	
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentPrefab));
			AssetDatabase.ImportAsset(path);

		}

     }

    static Object GetPrefab(GameObject go, string name)
    {
        Object tempPrefab = PrefabUtility.CreateEmptyPrefab("Assets/" + name + ".prefab");
        tempPrefab = PrefabUtility.ReplacePrefab(go, tempPrefab);
        Object.DestroyImmediate(go);
        return tempPrefab;
    }

    public static string AssetbundlePath
    {
        get 
		{ 
			return "Assets" + Path.DirectorySeparatorChar + "AssetBundles" + Path.DirectorySeparatorChar; 
		}
    }
	
	
}
