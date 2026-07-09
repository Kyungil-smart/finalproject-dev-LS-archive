using System;
using System.Collections.Generic;
using System.IO;
using Core;
using InGame.Sort.Data;
using UnityEngine;
using Utils;

namespace Lobby.Deck
{
    // 유저 기물 보유·레벨 상태 — (PieceName, PieceGrade) 그룹 단위. 그룹 안에서 Lv 1~5가 강화 단계.
    //   기본 1성 보유(Lv1) / 2·3성 미보유. 해금 → Owned=true, 강화 → Level++.
    //   저장은 RewardManager와 같은 방식(JsonUtility + Encrypter + persistentDataPath).
    //   JsonUtility가 Dictionary를 못 다뤄 List로 저장하고 런타임엔 Dictionary로 조회.
    // static — 공개 메서드마다 EnsureInit()으로 지연 초기화(데이터 테이블 기준 그룹 구성 + 세이브 병합).
    //   테이블 미로드 시 빈 맵으로 두고 다음 호출에 재시도 — 초기화 순서에 안 물림.
    // 작성자: 이성규
    public static class PieceInventory
    {
        [Serializable]
        private class Entry
        {
            public string Name;
            public int Grade;
            public bool Owned;
            public int Level;
        }

        [Serializable]
        private class SaveData
        {
            public List<Entry> Entries = new List<Entry>();
        }

        // 그룹 키 — (이름, 성급)
        private static Dictionary<(string, int), Entry> _map;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "inventory.dat");

        // 보유·레벨이 바뀌면 발행 — 덱 목록·강화창이 구독해 갱신.
        public static event Action OnChanged;

        // 데이터 테이블 기준으로 그룹 구성 후 세이브 병합.
        //   구성에 성공하면(Count > 0) 이후 호출은 즉시 반환. 실패 시 다음 호출에 재시도.
        public static void EnsureInit()
        {
            if (_map != null && _map.Count > 0) return;

            var table = DataManager.Instance != null
                ? DataManager.Instance.GetTable<PieceData>()
                : null;

            if (table == null)
            {
                // 테이블 아직 없음 — NRE만 막고 다음 호출에 재시도.
                if (_map == null) _map = new Dictionary<(string, int), Entry>();
                return;
            }

            _map = new Dictionary<(string, int), Entry>();

            // 1) 데이터에 존재하는 모든 그룹을 기본 상태로 생성
            foreach (var data in table.Values)
            {
                if (data == null) continue;

                var key = (data.PieceName, data.PieceGrade);
                if (_map.ContainsKey(key)) continue;

                _map[key] = new Entry
                {
                    Name = data.PieceName,
                    Grade = data.PieceGrade,
                    Owned = data.PieceGrade == 1,   // 1성만 기본 해금
                    Level = 1
                };
            }

            // 2) 세이브가 있으면 덮어씀 — 데이터에 없는 그룹은 무시(테이블 변경 대응)
            Load();
        }

        // ---- 조회 ----

        public static bool IsOwned(string name, int grade)
        {
            EnsureInit();
            return _map.TryGetValue((name, grade), out var e) && e.Owned;
        }

        public static int GetLevel(string name, int grade)
        {
            EnsureInit();
            return _map.TryGetValue((name, grade), out var e) ? e.Level : 1;
        }

        // ---- 변경 ----

        // 해금 — 미보유 그룹을 보유로. 이미 보유면 false(재화 소비는 호출부가 먼저 처리).
        public static bool Unlock(string name, int grade)
        {
            EnsureInit();

            if (!_map.TryGetValue((name, grade), out var e)) return false;
            if (e.Owned) return false;

            e.Owned = true;
            NotifyAndSave();
            return true;
        }

        // 강화 — 레벨 1 상승. 미보유거나 상한이면 false.
        //   상한은 데이터 기준이라 호출부가 PieceQuery.GetMaxLevel로 구해 넘김.
        public static bool LevelUp(string name, int grade, int maxLevel)
        {
            EnsureInit();

            if (!_map.TryGetValue((name, grade), out var e)) return false;
            if (!e.Owned || e.Level >= maxLevel) return false;

            e.Level++;
            NotifyAndSave();
            return true;
        }

        // ---- 저장·불러오기 (RewardManager와 동일 패턴) ----

        private static void NotifyAndSave()
        {
            OnChanged?.Invoke();
            Save();
        }

        private static void Save()
        {
            if (_map == null) return;

            try
            {
                var save = new SaveData();
                foreach (var e in _map.Values) save.Entries.Add(e);

                string json = JsonUtility.ToJson(save);
                File.WriteAllText(SavePath, Encrypter.Encrypt(json));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PieceInventory] 저장 실패: {ex.Message}");
            }
        }

        private static void Load()
        {
            if (!File.Exists(SavePath)) return;

            try
            {
                string decrypted = Encrypter.Decrypt(File.ReadAllText(SavePath));
                var save = JsonUtility.FromJson<SaveData>(decrypted);
                if (save?.Entries == null) return;

                foreach (var saved in save.Entries)
                {
                    // 데이터에 없는 그룹은 무시 — 테이블에서 빠진 기물의 세이브 잔여 방지
                    if (!_map.TryGetValue((saved.Name, saved.Grade), out var e)) continue;

                    e.Owned = saved.Owned;
                    e.Level = saved.Level;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PieceInventory] 불러오기 실패, 기본 상태 사용: {ex.Message}");
            }
        }
    }
}