﻿using System;
using System.Collections;
using System.Collections.Generic;
using Messages.Server.SoundMessages;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;


public class GlassShard : NetworkBehaviour, IServerSpawn
{
	[SyncVar(hook = nameof(SyncSpriteRotation))]
	private Quaternion spriteRotation;

	private SpriteRenderer spriteRenderer;
	private CustomNetTransform netTransform;
	private SpriteHandler spriteHandler;

	#region Lifecycle

	void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		netTransform = GetComponent<CustomNetTransform>();
		spriteHandler = GetComponentInChildren<SpriteHandler>();
	}

	public void OnSpawnServer(SpawnInfo info)
	{
		SetSpriteAndScatter(Random.Range(0, spriteHandler.CatalogueCount));
	}

	#endregion Lifecycle

	[Server]
	public void SetSpriteAndScatter(int index)
	{
		spriteHandler.ChangeSprite(index);
		netTransform?.SetPosition(netTransform.ServerState.WorldPosition + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f)));

		//Add a bit of rotation variance to the sprite obj:
		var axis = new Vector3(0, 0, 1);
		spriteRotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), axis);
	}

	private void SyncSpriteRotation(Quaternion oldValue, Quaternion newValue)
	{
		spriteRotation = newValue;

		if (spriteRenderer != null)
		{
			spriteRenderer.transform.localRotation = spriteRotation;
		}
	}

	// Serverside only - play glass crunching sound when stepped on
	public void OnTriggerEnter2D(Collider2D coll)
	{
		if (!isServer)
		{
			return;
		}

		//8 = Players layer
		if (coll.gameObject.layer == 8)
		{
			AudioSourceParameters audioSourceParameters = new AudioSourceParameters(pitch: Random.Range(0.8f, 1.2f));
			SoundManager.PlayNetworkedAtPos(CommonSounds.Instance.GlassStep, coll.transform.position, audioSourceParameters, sourceObj: gameObject);
		}
	}
}
