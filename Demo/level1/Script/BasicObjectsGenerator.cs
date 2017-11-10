﻿using System;

using UnityEngine;
using Cubizer;
using Cubizer.Math;

public class BasicObjectsGenerator : TerrainGenerator
{
	public bool _isGenTree = true;
	public bool _isGenWater = true;
	public bool _isGenCloud = true;
	public bool _isGenFlower = true;
	public bool _isGenWeed = true;
	public bool _isGenGrass = true;
	public bool _isGenObsidian = true;
	public bool _isGenPlaneOnly = false;

	public int _floorBase = 10;
	public int _floorHeightLismit = 20;

	public int _layerGrass = 0;
	public int _layerCloud = 3;
	public int _layerObsidian = -10;

	private VoxelMaterial _entityGrass;
	private VoxelMaterial _entityTree;
	private VoxelMaterial _entityTreeLeaf;
	private VoxelMaterial _entityFlower;
	private VoxelMaterial _entityWeed;
	private VoxelMaterial _entityObsidian;
	private VoxelMaterial _entityWater;
	private VoxelMaterial _entityCloud;

	public void Start()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			var entity = transform.GetChild(i);
			var renderer = transform.GetChild(i).GetComponent<MeshRenderer>();
			var descripter = transform.GetChild(i).GetComponent<TerrainEntityBehaviour>();
			var material = renderer.material;
			var name = transform.GetChild(i).name;

			if (!GameResources.RegisterMaterial(name, entity.gameObject))
				continue;

			var voxelMaterial = new VoxelMaterial(name, material.name, descripter.is_transparent, descripter.is_dynamic, descripter.is_merge);

			switch (name)
			{
				case "Grass":
					_entityGrass = voxelMaterial;
					break;

				case "Tree":
					_entityTree = voxelMaterial;
					break;

				case "TreeLeaf":
					_entityTreeLeaf = voxelMaterial;
					break;

				case "Flower":
					_entityFlower = voxelMaterial;
					break;

				case "Weed":
					_entityWeed = voxelMaterial;
					break;

				case "Obsidian":
					_entityObsidian = voxelMaterial;
					break;

				case "Water":
					_entityWater = voxelMaterial;
					break;

				case "Cloud":
					_entityCloud = voxelMaterial;
					break;
			}
		}
	}

	public override void OnCreateChunk(ChunkTree map)
	{
		var pos = map.position;

		int offsetX = pos.x * map.bound.x;
		int offsetY = pos.y * map.bound.y;
		int offsetZ = pos.z * map.bound.z;

		float layer = pos.y;

		if (_isGenPlaneOnly)
		{
			if (layer == _layerGrass)
			{
				byte h = (byte)_floorBase;
				for (byte x = 0; x < map.bound.x; x++)
				{
					for (byte z = 0; z < map.bound.z; z++)
					{
						for (byte y = 0; y < h; y++)
							map.Set(x, y, z, _entityGrass);
					}
				}
			}

			return;
		}

		if (layer == _layerGrass)
		{
			for (byte x = 0; x < map.bound.x; x++)
			{
				for (byte z = 0; z < map.bound.z; z++)
				{
					int dx = offsetX + x;
					int dz = offsetZ + z;

					float f = Noise.simplex2(dx * 0.01f, dz * 0.01f, 4, 0.4f, 2);

					byte h = (byte)(f * (f * _floorHeightLismit + _floorBase));

					if (_isGenGrass)
					{
						for (byte y = 0; y < h; y++)
							map.Set(x, y, z, _entityGrass);
					}

					if (_isGenWater && h <= _floorBase - _floorHeightLismit * 0.2f)
					{
						for (byte y = h; y <= _floorBase - _floorHeightLismit * 0.2f; y++)
							map.Set(x, y, z, _entityWater);
					}
					else
					{
						if (_isGenWeed && Noise.simplex2(-dx * 0.1f, dz * 0.1f, 4, 0.8f, 2) > 0.7)
							map.Set(x, h, z, _entityWeed);
						else if (_isGenFlower && Noise.simplex2(dx * 0.05f, -dz * 0.05f, 4, 0.8f, 2) > 0.75)
							map.Set(x, h, z, _entityFlower);
						else if (_isGenTree && h < map.bound.y - 8)
						{
							if (x > 3 && x < map.bound.y - 3 && z > 3 && z < map.bound.y - 3)
							{
								if (Noise.simplex2(dx, dz, 6, 0.5f, 2) > 0.84)
								{
									for (int y = h + 3; y < h + 8; y++)
									{
										for (int ox = -3; ox <= 3; ox++)
										{
											for (int oz = -3; oz <= 3; oz++)
											{
												int d = (ox * ox) + (oz * oz) + (y - (h + 4)) * (y - (h + 4));
												if (d < 11)
													map.Set((byte)(x + ox), (byte)y, (byte)(z + oz), _entityTreeLeaf);
											}
										}
									}

									for (byte y = h; y < h + 7; y++)
										map.Set(x, y, z, _entityTree);
								}
							}
						}
					}
				}
			}
		}

		if (_isGenCloud && layer == _layerCloud)
		{
			for (int x = 0; x < map.bound.x; x++)
			{
				for (int z = 0; z < map.bound.y; z++)
				{
					int dx = offsetX + x;
					int dz = offsetZ + z;

					for (int y = 0; y < 8; y++)
					{
						int dy = offsetY + y;

						if (Noise.simplex3(dx * 0.01f, dy * 0.1f, dz * 0.01f, 8, 0.5f, 2) > 0.75)
							map.Set((byte)x, (byte)y, (byte)z, _entityCloud);
					}
				}
			}
		}

		if (_isGenObsidian && layer == _layerObsidian)
		{
			for (byte x = 0; x < map.bound.x; x++)
			{
				for (byte z = 0; z < map.bound.z; z++)
				{
					for (byte y = 0; y < 8; y++)
						map.Set(x, y, z, _entityObsidian);
				}
			}
		}
	}
}