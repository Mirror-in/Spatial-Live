using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    AudioSource audioSource;
    public string spreadsheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQZhduwMlaYrXwIr9FdqUY4v4_XC8KVfZdVE3mAUrEP56uyhRq-ykWHUvLJMMWRJxOzkWG7GLJE8N2F/pub?output=csv";
    List<string> musicUrls = new List<string>();
    List<string> newMusicUrls; // 新しいリストを一時的に保持
    List<string> playedMusicUrls = new List<string>(); // 再生済みリスト
    bool isPlaying = false;
    public float updateInterval = 180f; // 更新間隔（秒）

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(UpdateMusicListPeriodically());
        StartCoroutine(PlayMusicFromSpreadsheet());
    }

    void Update()
    {
        if (!audioSource.isPlaying && isPlaying)
        {
            isPlaying = false;
            if (newMusicUrls != null)
            {
                // 新しいリストがあればそれを使用
                musicUrls = new List<string>(newMusicUrls);
                playedMusicUrls.Clear(); // 再生済みリストをクリア
                newMusicUrls = null;
            }
            PlayRandomMusic();
        }
    }

    IEnumerator UpdateMusicListPeriodically()
    {
        while (true)
        {
            yield return StartCoroutine(DownloadMusicList());
            yield return new WaitForSeconds(updateInterval);
        }
    }

    IEnumerator DownloadMusicList()
    {
        WWW www = new WWW(spreadsheetUrl);
        yield return www;

        string data = www.text;
        newMusicUrls = ParseSpreadsheetData(data); // 新しいリストをダウンロード
    }

    IEnumerator PlayMusicFromSpreadsheet()
    {
        yield return StartCoroutine(DownloadMusicList());
        PlayRandomMusic();
    }

    void PlayRandomMusic()
    {
        if (musicUrls.Count == 0)
        {
            if (newMusicUrls != null)
            {
                // 新しいリストがあればそれを使用
                musicUrls = new List<string>(newMusicUrls);
                playedMusicUrls.Clear();
                newMusicUrls = null;
            }
            else
            {
                // 新しいリストがなければ、再生済みリストをリセット
                musicUrls.AddRange(playedMusicUrls);
                playedMusicUrls.Clear();
            }
        }

        if (musicUrls.Count > 0)
        {
            int randomIndex = Random.Range(0, musicUrls.Count);
            string randomMusicUrl = musicUrls[randomIndex];
            playedMusicUrls.Add(randomMusicUrl); // 再生済みリストに追加
            musicUrls.RemoveAt(randomIndex); // リストから削除

            StartCoroutine(PlayMusic(randomMusicUrl));
        }
    }

    IEnumerator PlayMusic(string url)
    {
        WWW musicWww = new WWW(url);
        yield return musicWww;

        audioSource.clip = musicWww.GetAudioClip(false, false, AudioType.WAV);
        audioSource.Play();
        isPlaying = true;
    }

    List<string> ParseSpreadsheetData(string data)
    {
        List<string> urls = new List<string>();

        string[] rows = data.Split('\n');
        foreach (var row in rows)
        {
            if (!string.IsNullOrEmpty(row))
            {
                string[] columns = row.Split(',');
                string url = columns[0].Trim();
                urls.Add(url);
            }
        }
        return urls;
    }
}
