/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Immersal.AR;
using Immersal.REST;

namespace Immersal.Samples
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class MapListController : MonoBehaviour
    {
        [SerializeField]
        private ARMap m_ARMap;
        
        private ISceneUpdateable m_SceneParent;
        private List<SDKJob> m_Maps;
        private TMP_Dropdown m_Dropdown;
        private List<JobAsync> m_Jobs = new List<JobAsync>();
        private int m_JobLock = 0;
        private TextAsset m_EmbeddedMapFile;
        private Mesh m_EmbeddedMesh;

        void Awake()
        {
            m_Dropdown = GetComponent<TMP_Dropdown>();
            m_Dropdown.ClearOptions();
            bool validEmbed = false;
            
            // check for embedded map
            if (m_ARMap != null && m_ARMap.IsConfigured)
            {
                // fetch scene parent
                if (MapManager.TryGetMapEntry(m_ARMap.mapId, out MapEntry entry))
                {
                    m_SceneParent = entry.SceneParent;
                }
                else
                {
                    Debug.LogError("Could not find map entry for embedded map");
                    enabled = false;
                    return;
                }

                if (m_ARMap.mapFile != null)
                {
                    m_Dropdown.AddOptions( new List<string>() { string.Format("<{0}>", m_ARMap.mapFile.name) });
                    m_EmbeddedMapFile = m_ARMap.mapFile;
                    if (m_ARMap.Visualization.Mesh != null)
                    {
                        m_EmbeddedMesh = m_ARMap.Visualization.Mesh;
                    }
                    validEmbed = true;
                }
            }
            else // instantiate if not provided
            {
                GameObject space = new GameObject("AR Space");
                m_SceneParent = space.AddComponent<ARSpace>();
                
                GameObject map = new GameObject("AR Map");
                m_ARMap = map.AddComponent<ARMap>();
                map.transform.SetParent(space.transform);
            }
            
            if (!validEmbed)
            {
                m_Dropdown.AddOptions( new List<string>() { "Load map..." });
            }

            m_Maps = new List<SDKJob>();
        }

        void Start()
        {
            Invoke("GetMaps", 0.5f);
        }

        void Update()
        {
            if (m_JobLock == 1)
                return;
            
            if (m_Jobs.Count > 0)
            {
                m_JobLock = 1;
                RunJob(m_Jobs[0]);
            }
        }

        public async void OnValueChanged(TMP_Dropdown dropdown)
        {
            int value = dropdown.value - 1;
            
            // remove current
            if (m_ARMap != null && m_ARMap.IsConfigured)
                MapManager.RemoveMap(m_ARMap.mapId);
            
            // use embedded map
            if (m_EmbeddedMapFile != null && value == -1)
            {
                m_ARMap.Configure(m_EmbeddedMapFile);
                await MapManager.RegisterAndLoadMap(m_ARMap, m_SceneParent);
                
                // vis
                if (m_EmbeddedMesh != null)
                {
                    m_ARMap.CreateVisualization();
                    m_ARMap.Visualization.SetMesh(m_EmbeddedMesh);
                }
                else
                {
                    JobLoadMapSparseAsync vj = new JobLoadMapSparseAsync();
                    vj.id = m_ARMap.mapId;
                    vj.OnResult += (SDKSparseDownloadResult result) =>
                    {
                        m_ARMap.CreateVisualization(ARMapVisualization.RenderMode.EditorAndRuntime);
                        m_ARMap.Visualization.LoadPly(result.data, m_ARMap.mapName);
                    };
                    m_Jobs.Add(vj);
                }
            }
            else
            {
                if (value >= 0)
                {
                    SDKJob map = m_Maps[value];
                    LoadMap(map);
                }
            }
        }

        public void GetMaps()
        {
            JobListJobsAsync j = new JobListJobsAsync();
            j.token = ImmersalSDK.Instance.developerToken;
            j.OnResult += (SDKJobsResult result) =>
            {
                if (result.count > 0)
                {
                    List<string> names = new List<string>();

                    foreach (SDKJob job in result.jobs)
                    {
                        if (job.type != (int)SDKJobType.Alignment && (job.status == SDKJobState.Sparse || job.status == SDKJobState.Done))
                        {
                            this.m_Maps.Add(job);
                            names.Add(job.name);
                        }
                    }

                    this.m_Dropdown.AddOptions(names);
                }
            };

            m_Jobs.Add(j);
        }

        public void ClearMaps()
        {
            MapManager.RemoveAllMaps();
            m_Dropdown.SetValueWithoutNotify(0);
        }

        public void LoadMap(SDKJob job)
        {
            JobLoadMapBinaryAsync j = new JobLoadMapBinaryAsync();
            j.id = job.id;
            j.OnResult += async (SDKMapResult result) =>
            {
                Debug.LogFormat("Load map {0} ({1} bytes)", job.id, result.mapData.Length);
                m_ARMap.Configure(result.metadata, true);
                await MapManager.RegisterAndLoadMap(m_ARMap, m_SceneParent, result.mapData);
                
                // vis
                JobLoadMapSparseAsync vj = new JobLoadMapSparseAsync();
                vj.id = job.id;
                vj.OnResult += (SDKSparseDownloadResult plyResult) =>
                {
                    m_ARMap.CreateVisualization();
                    m_ARMap.Visualization.LoadPly(plyResult.data, result.metadata.name);
                };
                m_Jobs.Add(vj);
            };
            j.OnError += (e) =>
            {
                Debug.LogError(e);
            };

            m_Jobs.Add(j);
        }

        private async void RunJob(JobAsync j)
        {
            await j.RunJobAsync();
            if (m_Jobs.Count > 0)
                m_Jobs.RemoveAt(0);
            m_JobLock = 0;
        }
    }
}
