using UnityEngine;
using System.Collections;

namespace lnxMazeGen{

public class MazeTileObj : MonoBehaviour {

	public MazeTile tileInfo;
	public SpriteRenderer spriteRenderer;

	public void SetTileGraphic(){
		spriteRenderer.sprite = MazeGenSpriteStore.singleton.LookupSprite (tileInfo);
	}

}
}