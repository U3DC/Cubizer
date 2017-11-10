﻿using System.Collections;

using UnityEngine;
using UnityEditor;

namespace Cubizer
{
	[CustomEditor(typeof(TerrainData))]
	public class TerrainDataInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainData chunk = (TerrainData)target;

			base.DrawDefaultInspector();

			EditorGUILayout.HelpBox("Load & Save chunk of terrain from Asset", MessageType.Info);

			if (GUILayout.Button("Load..."))
			{
				var SelectedPath = EditorUtility.OpenFilePanel("Load Chunk", "", "asset");
				if (SelectedPath.Length == 0)
					return;

				var map = ChunkTree.Load(SelectedPath, chunk.map.manager);
				if (map != null)
				{
					map.position = chunk.map.position;
					chunk.map = map;
					chunk.UpdateChunk();

					Debug.Log("Your data of chunk was loaded successfully");
				}
				else
				{
					Debug.Log("load Failed");
				}
			}

			if (GUILayout.Button("Save..."))
			{
				var SelectedPath = EditorUtility.SaveFilePanel("Save Chunk", "", "New Resource", "asset");
				if (SelectedPath.Length == 0)
					return;

				var map = chunk.map;
				if (map != null)
				{
					if (ChunkTree.Save(SelectedPath, map))
						Debug.Log("Your data of chunk was saved successfully");
					else
						Debug.Log("Save Failed");
				}
			}
		}
	}
}