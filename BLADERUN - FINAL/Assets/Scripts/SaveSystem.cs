using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static int selectedSlot = 1;

    private static string SaveFolder
    {
        get
        {
            string gameFolder =
                Directory.GetParent(
                    UnityEngine.Application.dataPath
                ).FullName;

            return Path.Combine(
                gameFolder,
                "Saves"
            );
        }
    }

    private static string GetSavePath(int slot)
    {
        return Path.Combine(
            SaveFolder,
            "save" + slot + ".json"
        );
    }

    public static void SelectSlot(int slot)
    {
        selectedSlot = slot;

        SaveData data = LoadSlot(slot);

        if (data != null)
        {
            GameTimer.SetTime(data.elapsedTime);

            SceneManager.LoadScene(
                data.sceneName
            );
        }
        else
        {
            GameTimer.ResetTimer();

            SaveSceneForSlot(
                slot,
                "1Forest"
            );

            SceneManager.LoadScene(
                "1Forest"
            );
        }
    }

    public static void SaveScene(string sceneName)
    {
        SaveSceneForSlot(
            selectedSlot,
            sceneName
        );
    }

    private static void SaveSceneForSlot(int slot, string sceneName)
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        SaveData data = new SaveData();

        data.slot = slot;
        data.sceneName = sceneName;
        data.elapsedTime = GameTimer.GetTime();

        data.lastSaved =
            System.DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss"
            );

        string json =
            JsonUtility.ToJson(
                data,
                true
            );

        File.WriteAllText(
            GetSavePath(slot),
            json
        );

        DatabaseManager.SaveSlot(data);
    }

    public static SaveData LoadSlot(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
            return null;

        string json =
            File.ReadAllText(path);

        return JsonUtility.FromJson<SaveData>(
            json
        );
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(
            GetSavePath(slot)
        );
    }

    public static void DeleteSlot(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
            File.Delete(path);

        DatabaseManager.DeleteSlot(slot);
    }
}