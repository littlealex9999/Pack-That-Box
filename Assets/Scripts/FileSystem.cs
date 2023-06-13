using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileSystem
{
    #region Helper Functions
    static string DeterminePath(string filename) => Application.dataPath + "/" + filename;

    static BinaryWriter OpenWriter(string filename, out Stream stream)
    {
        string path = DeterminePath(filename);

        // either overwrites the old content, or creates a new file.
        FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

        // files get saved in the application directory
        stream = File.Open(path, mode);
        BinaryWriter bw = new BinaryWriter(stream);

#if UNITY_EDITOR
        // only output this for debug purposes
        Debug.Log("Writing a file at: " + path);
#endif

        return bw;
    }

    static BinaryReader OpenReader(string filename, out Stream stream)
    {
        string path = DeterminePath(filename);

        if (File.Exists(path)) {
            stream = File.Open(path, FileMode.Open);
            BinaryReader br = new BinaryReader(stream);

#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Reading a file at: " + path);
#endif

            return br;
        } else { // failed to load the file as it doesn't exist
#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Failed to find a file at: " + path);
#endif
        }

        stream = null;
        return null;
    }

    static void CloseSystem(BinaryReader binaryIO, Stream stream)
    {
        binaryIO.Close();
        stream.Close();
    }
    static void CloseSystem(BinaryWriter binaryIO, Stream stream)
    {
        binaryIO.Close();
        stream.Close();
    }
    #endregion

    public static void SaveFile(string filename, Score score)
    {
        BinaryWriter bw = OpenWriter(filename, out Stream s);

        bw.Write(score.Length);
        for (int i = 0; i < score.Length; ++i) {
            bw.Write(score[i]);
        }

        CloseSystem(bw, s);
    }

    public static bool LoadFile(string filename, out Score score)
    {
        BinaryReader br = OpenReader(filename, out Stream s);

        if (br != null) {
            float[] scoresValues = new float[br.ReadInt32()];

            for (int i = 0; i < scoresValues.Length; ++i) {
                scoresValues[i] = br.ReadSingle();
            }

            score = new Score(true, scoresValues);
            CloseSystem(br, s);
            return true;
        }

        score = new Score();
        return false;
    }
}
