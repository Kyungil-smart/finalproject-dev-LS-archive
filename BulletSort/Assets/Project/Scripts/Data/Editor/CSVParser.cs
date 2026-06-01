using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CSV 파일을 Parsing하여 데이터를 SO 파일로 저장하기 위한 Editor 코드.
/// SO파일의 이름은 CSV 파일의 이름과 완전히 일치해야 함.
/// 
/// Unity 프로세스 실행 시 SO 파일을 런타임 인스턴스로 만드는 기능은 여기에 포함되지 않음.
/// 
/// 작성자 : 김경민
/// </summary>

public class CSVParser
{
    [MenuItem("Data Tools/Parse CSV Files")]

    public static void ParseCSV()
    {
        string csvDirectoryPath = Path.Combine(Application.dataPath, "Resources/CSVTables");
        string outputPath = "Assets/Resources/SO";

        if (!Directory.Exists(csvDirectoryPath))
        {
            Debug.LogError($"CSVParser : CSV Directory is Not Found : {csvDirectoryPath}");
            return;
        }

        string[] csvFiles = Directory.GetFiles(csvDirectoryPath, "*.csv");

        foreach (string csvFile in csvFiles)
        {
            string className = Path.GetFileNameWithoutExtension(csvFile);

            // csv 파일 이름(확장자 제외)와 SO 클래스의 이름이 완전히 일치해야 함.
            Type soType = Type.GetType(className);

            if (soType == null)
            {
                soType = GetTypeFromName(className);
                if (soType == null)
                {
                    Debug.LogWarning($"CSVParser : SO Class is Not Found, Skipped Parsing : {soType}");
                    continue;
                }
            }

            string targetFolder = $"{outputPath}/{className}";

            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                if (!AssetDatabase.IsValidFolder(outputPath))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "SO");
                }

                AssetDatabase.CreateFolder(outputPath, className);
            }

            // Read Each Line of CSV File

            string[] lines = File.ReadAllLines(csvFile);
            if (lines.Length <= 1)
            {
                Debug.LogWarning($"CSVParser : Invalid File : {csvFile}");
                continue;
            }

            string[] headers = lines[0].Split(',');

            for (int iCnt = 0; iCnt < headers.Length; ++iCnt)
            {
                headers[iCnt] = headers[iCnt].Trim();
            }

            // 첫 column의 이름이 key로 간주
            string idFieldName = headers[0];

            for (int lineIdx = 1; lineIdx < lines.Length; ++lineIdx)
            {
                if (string.IsNullOrWhiteSpace(lines[lineIdx]))
                {
                    continue;
                }

                string[] row = lines[lineIdx].Split(',');
                string idStr = row[0].Trim();

                // 에셋 파일 명 예시 : Assets/Resources/SO/DataTypeName/DataTypeName_ID.asset
                string assetPath = $"{targetFolder}/{className}_{idStr}.asset";

                // SO 로드 or 생성
                ScriptableObject soInstance = AssetDatabase.LoadAssetAtPath(assetPath, soType) as ScriptableObject;
                if (soInstance != null)
                {
                    soInstance = ScriptableObject.CreateInstance(soType);
                    AssetDatabase.CreateAsset(soInstance, assetPath);
                }

                // csv에서 값 찾고 SO에 넣어주기
                for (int headerIdx = 0; headerIdx < headers.Length; ++headerIdx)
                {
                    string fieldName = headers[headerIdx];
                    string value = row[headerIdx].Trim();

                    FieldInfo field = soType.GetField(fieldName);
                    if (field != null)
                    {
                        object convertedValue = ConvertType(value, field.FieldType);
                        field.SetValue(soInstance, convertedValue);
                    }
                    else
                    {
                        Debug.LogWarning($"CSVParser : Invalid Field Name : {csvFile}, {fieldName}");
                    }
                }

                EditorUtility.SetDirty(soInstance);

                Debug.Log($"CSVParser : Parsing Success : Row {lineIdx} in {csvFile} to {assetPath}");
            }

            Debug.Log($"CSVParser : Parsing one File is Done : {csvFile}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"CSVParser : All Files are Done");
    }

    private static Type GetTypeFromName(string className)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = assembly.GetType(className);

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    private static object ConvertType(string value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(int))
        {
            return int.Parse(value);
        }

        if (targetType == typeof(float))
        {
            return float.Parse(value);
        }

        if (targetType == typeof(bool))
        {
            return bool.Parse(value);
        }

        return Convert.ChangeType(value, targetType);
    }
}
