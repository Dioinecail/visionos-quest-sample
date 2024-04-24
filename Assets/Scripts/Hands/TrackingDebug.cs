using Project.Hands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Project.Debugging
{
    [System.Serializable]
    public class TrackingArray
    {
        public List<TrackingBatch> Batchs;
    }

    [System.Serializable]
    public class TrackingBatch
    {
        public TrackingData[] Batch;
    }

    [System.Serializable]
    public class TrackingData
    {
        public int JointIndex;
        public float[] Position;
        public float[] Rotation;
    }

    public class TrackingDebug : MonoBehaviour
    {
        [SerializeField] private string m_SavePath;
        [SerializeField] private TextAsset m_TextAsset;
        [SerializeField] private bool m_Record;
        [SerializeField] private bool m_Replay;
        [SerializeField] private bool m_ReplayOnAwake;
        [SerializeField] private TMP_Text m_DebugText;
        [SerializeField] private AdaptiveHandSkeletonDriver m_HandSkeletonDriver;
        [SerializeField] private List<JointToTransformReference> m_JointTransformReferences;
        [SerializeField] private int m_BatchIndex;

        private TrackingArray m_SavedData = new TrackingArray() { Batchs = new List<TrackingBatch>() };



        private void Awake()
        {
            if (m_ReplayOnAwake)
            {
                Load();
                StartCoroutine(DoReplay());

                m_Replay = true;
            }
        }

        private void Update()
        {
            if (!m_Record)
            {
                if (!m_Replay)
                    return;

                var replayBatch = m_SavedData.Batchs[m_BatchIndex];

                for (int i = 0; i < m_JointTransformReferences.Count; i++)
                {
                    var replayData = replayBatch.Batch[i];
                    var jointId = (XRHandJointID)replayData.JointIndex;
                    var joint = m_JointTransformReferences.FirstOrDefault(j => j.xrHandJointID == jointId);

                    var Position = new Vector3(replayData.Position[0], replayData.Position[1], replayData.Position[2]);
                    var Rotation = new Quaternion(replayData.Rotation[0], replayData.Rotation[1], replayData.Rotation[2], replayData.Rotation[3]);

                    joint.jointTransform.position = Position;
                    joint.jointTransform.rotation = Rotation;
                }

                return;
            }

            var data = new TrackingData[m_HandSkeletonDriver.jointTransformReferences.Count];

            for (int i = 0; i < data.Length; i++)
            {
                var joint = m_HandSkeletonDriver.jointTransformReferences[i];
                var pos = joint.jointTransform.position;
                var rot = joint.jointTransform.rotation;

                data[i] = new TrackingData
                {
                    JointIndex = (int)joint.xrHandJointID,
                    Position = new float[3] { pos[0], pos[1], pos[2] },
                    Rotation = new float[4] { rot[0], rot[1], rot[2], rot[3] }
                };
            }

            var batch = new TrackingBatch()
            {
                Batch = data
            };

            m_SavedData.Batchs.Add(batch);
        }

        [ContextMenu("Save")]
        private void Save()
        {
            var dataJson = JsonUtility.ToJson(m_SavedData);

            if (File.Exists(m_SavePath))
            {
                File.Delete(m_SavePath);
            }

            File.WriteAllText(m_SavePath, dataJson);
        }

        [ContextMenu("Load")]
        private void Load()
        {
            var dataJson = string.Empty;

            if (m_TextAsset != null)
            {
                dataJson = m_TextAsset.text;
            }

            if (string.IsNullOrEmpty(dataJson) && File.Exists(m_SavePath))
            {
                dataJson = File.ReadAllText(m_SavePath);
            }

            if (!string.IsNullOrEmpty(dataJson))
                m_SavedData = JsonUtility.FromJson<TrackingArray>(dataJson);
        }

        [ContextMenu("Replay")]
        private void Replay()
        {
            if (m_SavedData == null || m_SavedData.Batchs.Count == 0)
                return;

            StartCoroutine(DoReplay());
        }

        [ContextMenu("Copy")]
        private void Copy()
        {
            m_JointTransformReferences = m_HandSkeletonDriver.jointTransformReferences;
        }

        private IEnumerator DoReplay()
        {
            while (true)
            {
                if (m_BatchIndex >= m_SavedData.Batchs.Count - 1)
                    m_BatchIndex = 0;

                m_BatchIndex++;

                yield return null;
            }
        }
    }
}