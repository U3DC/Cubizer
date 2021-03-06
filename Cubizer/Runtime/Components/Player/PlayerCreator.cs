﻿using System.Collections;

using UnityEngine;
using Cubizer.Chunk;

namespace Cubizer.Players
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Cubizer/PlayerCreator")]
	public class PlayerCreator : MonoBehaviour, IPlayer
	{
		[SerializeField]
		private CubizerBehaviour _world;

		[SerializeField]
		private Camera _player;

		[SerializeField]
		private PlayerModels _model;

		[SerializeField]
		private LiveBehaviour _block;

		[SerializeField]
		private Mesh _drawPickMesh;

		[SerializeField]
		private Material _drawPickMaterial;

		[SerializeField] private bool _isHitTestEnable = true;
		[SerializeField] private bool _isHitTestWireframe = true;
		[SerializeField] private bool _isHitTesting = false;

		[SerializeField] private int _hitTestDistance = 8;

		public Camera player
		{
			get { return _player; }
		}

		public PlayerModels model
		{
			get { return _model; }
		}

		public Mesh drawPickMesh
		{
			set { _drawPickMesh = value; }
			get { return _drawPickMesh; }
		}

		public Material drawPickMaterial
		{
			set { _drawPickMaterial = value; }
			get { return _drawPickMaterial; }
		}

		public LiveBehaviour block
		{
			set { _block = value; }
			get { return _block; }
		}

		public bool isHitTestEnable
		{
			set { _isHitTestEnable = value; }
			get { return _isHitTestEnable; }
		}

		public bool isHitTestWireframe
		{
			set { _isHitTestWireframe = value; }
			get { return _isHitTestWireframe; }
		}

		public int hitTestDistance
		{
			set { _hitTestDistance = value; }
			get { return _hitTestDistance; }
		}

		public void Start()
		{
			if (_world == null)
				_world = GetComponent<CubizerBehaviour>();

			if (_world == null)
				Debug.LogError("Please assign a server on the inspector.");

			if (_player == null)
				Debug.LogError("Please assign a camera on the inspector.");

			if (_drawPickMesh == null)
				Debug.LogError("Please assign a material on the inspector");

			if (_drawPickMaterial == null)
				Debug.LogError("Please assign a mesh on the inspector");

			_world.players.Connection(this);
		}

		public void OnDestroy()
		{
			_world.players.Disconnect(this);
		}

		public void Reset()
		{
			_model.Reset();

			_isHitTesting = false;
			_isHitTestEnable = _model.settings.hitTestEnable;
			_isHitTestWireframe = _model.settings.hitTestWireframe;
			_hitTestDistance = _model.settings.hitTestDistance;

			StopCoroutine("AddEnitiyByScreenPosWithCoroutine");
			StopCoroutine("RemoveEnitiyByScreenPosWithCoroutine");
		}

		private void OnApplicationFocus(bool focus)
		{
			_isHitTesting = false;
		}

		private void OnApplicationPause(bool pause)
		{
			_isHitTesting = false;
		}

		private void Update()
		{
			if (Cursor.visible)
				UnityEngine.Time.timeScale = 0;
			else
				UnityEngine.Time.timeScale = 1;

			_model.SetTransform(transform);
		}

		private void LateUpdate()
		{
			this.UpdateChunkForHit(_drawPickMesh, _drawPickMaterial);
		}

		private void UpdateChunkForHit(Mesh mesh, Material material)
		{
			if (_isHitTestEnable)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					if (Input.GetMouseButton(0))
						StartCoroutine("RemoveEnitiyByScreenPosWithCoroutine");
					else if (Input.GetMouseButton(1))
						StartCoroutine("AddEnitiyByScreenPosWithCoroutine");
				}
			}

			if (_isHitTestWireframe)
			{
				byte x, y, z;
				ChunkPrimer chunk;

				var ray = _player.ScreenPointToRay(Input.mousePosition);
				ray.origin = _player.transform.position;

				if (_world.chunkManager.HitTestByRay(ray, _hitTestDistance, out chunk, out x, out y, out z))
				{
					var position = new Vector3(chunk.position.x, chunk.position.y, chunk.position.z) * _world.profile.chunk.settings.chunkSize + new Vector3(x, y, z);
					Graphics.DrawMesh(mesh, position, Quaternion.identity, material, gameObject.layer, _player);
				}
			}
		}

		private IEnumerator AddEnitiyByScreenPosWithCoroutine()
		{
			if (_isHitTesting)
				yield break;

			_isHitTesting = true;

			if (_block != null)
			{
				var material = VoxelMaterialManager.GetInstance().GetMaterial(_block.name);
				if (material != null)
					_world.chunkManager.AddBlockByScreenPos(Input.mousePosition, _hitTestDistance, material);
			}

			yield return new WaitWhile(() => Input.GetMouseButton(1));

			_isHitTesting = false;
		}

		private IEnumerator RemoveEnitiyByScreenPosWithCoroutine()
		{
			if (_isHitTesting)
				yield break;

			_isHitTesting = true;
			_world.chunkManager.RemoveBlockByScreenPos(Input.mousePosition, _hitTestDistance);

			yield return new WaitWhile(() => Input.GetMouseButton(0));

			_isHitTesting = false;
		}
	}
}