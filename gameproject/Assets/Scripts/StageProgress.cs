using UnityEngine;

public static class StageProgress
{
    private const string LastSelectedStageIndexKey = "LastSelectedStageIndex";
    private const string LastClearedStageIndexKey = "LastClearedStageIndex";

    public static void SaveLastSelectedStageIndex(int index)
    {
        PlayerPrefs.SetInt(LastSelectedStageIndexKey, index);
        PlayerPrefs.Save();
    }

    public static int LoadLastSelectedStageIndex()
    {
        return PlayerPrefs.GetInt(LastSelectedStageIndexKey, 0);
    }

    public static void SaveLastClearedStageIndex(int index)
    {
        int savedIndex = LoadLastClearedStageIndex();

        // 더 높은 스테이지만 저장
        if (index > savedIndex)
        {
            PlayerPrefs.SetInt(LastClearedStageIndexKey, index);
            PlayerPrefs.Save();
        }
    }

    public static int LoadLastClearedStageIndex()
    {
        return PlayerPrefs.GetInt(LastClearedStageIndexKey, -1);
    }
}