using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Facade;

public class EnemyPanelUI : BaseFacePanelUI
{
    public override void InitializeWithIndex(int index)
    {
        base.InitializeWithIndex(index);

        AssetReference enemy = null;
        if (mapSceneManager.NextFightNode.waves[0].Enemies.Length > index)
        {
            enemy = mapSceneManager.NextFightNode.waves[0].Enemies[index];
        }

        var GUID = enemy == null ? "" : enemy.AssetGUID;
        if (string.IsNullOrEmpty(GUID))
        {
            SetActive(false);
        }
        else
        {
            StartCoroutine(LoadMaggot(null, GUID));
        }

        inPartyCG.alpha = 0;
    }

    protected override IEnumerator LoadMaggot(PlayerSave.MaggotState state, string GUID)
    {
        var op = Addressables.LoadAssetAsync<CharacterObject>(GUID);
        yield return op;
        gachaSystem.TryAddLoadedMaggot(op);
        healthBar.fillAmount = 1;
        profileGraphic.sprite = op.Result.headshotSprite;
        canvas.Raycaster.enabled = false;
        SetActive(true);
    }
}
