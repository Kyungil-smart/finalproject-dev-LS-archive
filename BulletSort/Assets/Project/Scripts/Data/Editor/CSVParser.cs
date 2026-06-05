using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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
                    AssetDatabase.CreateFolder("Assets/Data", "SO");
                }

                AssetDatabase.CreateFolder(outputPath, className);
            }

            // Read Each Line of CSV File
            string[] lines = File.ReadAllLines(csvFile);
            if (lines.Length <= 2)
            {
                Debug.LogWarning($"CSVParser : Invalid File : {csvFile}");
                continue;
            }

            string[] rawHeaders = lines[1].Split(',');
            List<string> headers = new List<string>();

            foreach (string rawHeader in rawHeaders)
            {
                string header = rawHeader.Trim();

                if (header == "*")
                {
                    break;
                }

                headers.Add(header);
            }

            for (int lineIdx = 2; lineIdx < lines.Length; ++lineIdx)
            {
                if (string.IsNullOrWhiteSpace(lines[lineIdx]))
                {
                    continue;
                }

                string[] row = lines[lineIdx].Split(',');

                if (row.Length == 0 || row[0].Trim() == "" || row[0].Trim() == "*")
                {
                    continue;
                }

                string idStr = row[0].Trim();

                // 에셋 파일 명 예시 : Assets/Resources/SO/DataTypeName/DataTypeName_ID.asset
                string assetPath = $"{targetFolder}/{className}_{idStr}.asset";
                Debug.Log($"Asset Path : {assetPath}");

                // SO 로드 or 생성
                ScriptableObject soInstance = AssetDatabase.LoadAssetAtPath(assetPath, soType) as ScriptableObject;
                if (soInstance == null)
                {
                    soInstance = ScriptableObject.CreateInstance(soType);
                    AssetDatabase.CreateAsset(soInstance, assetPath);
                }

                // csv에서 값 찾고 SO에 넣어주기
                for (int headerIdx = 0; headerIdx < headers.Count; ++headerIdx)
                {
                    if (headerIdx >= row.Length)
                    {
                        break;
                    }

                    string fieldName = headers[headerIdx];
                    string value = row[headerIdx].Trim();

                    if (value == "*")
                    {
                        break;
                    }

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

        GenerateDataManagerPaths();
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

    private static void GenerateDataManagerPaths()
    {
        string soRootPath = Path.Combine(Application.dataPath, "Resources/SO");

        string dataManagerPath = Path.Combine(Application.dataPath, "Project/Scripts/Core/Managers/DataManager.cs");

        if (!Directory.Exists(soRootPath))
        {
            Debug.LogError($"CSV Parser : Path Error, DataManager.cs File is Not Found. Wrong Path : {soRootPath}");
        }

        if (!File.Exists(dataManagerPath))
        {
            Debug.LogError($"CSV Parser : Path Error, DataManager.cs File is Not Found. Wrong Path : {dataManagerPath}");
        }

        string[] directories = Directory.GetDirectories(soRootPath);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("            // [AUTO GENERATED START]");
        foreach (string dir in directories)
        {
            string folderName = Path.GetFileName(dir);
            sb.AppendLine($"            LoadTable<{folderName}>(\"SO/{folderName}\");");
        }
        sb.Append("            // [AUTO GENERATED END]");

        string originalCode = File.ReadAllText(dataManagerPath, Encoding.UTF8);

        string startMakrer = "            // [AUTO GENERATED START]";
        string endMarker = "            // [AUTO GENERATED END]";

        int startIndex = originalCode.IndexOf(startMakrer);
        int endIndex = originalCode.IndexOf(endMarker) + endMarker.Length;

        if (startIndex != -1 && endIndex != -1)
        {
            string firstPart = originalCode.Substring(0, startIndex);
            string secondPart = originalCode.Substring(endIndex);

            File.WriteAllText(dataManagerPath, firstPart + sb.ToString() + secondPart, Encoding.UTF8);

            AssetDatabase.Refresh();

            Debug.Log($"CSV Parser : DataManager Code is Successfully Generated");
        }
        else
        {
            Debug.LogError($"CSV Parser : Start or End Marker is Not Found in DataManager.cs File");
        }
    }
}
