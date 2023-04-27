using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;
using static Facade;

public class ShowcaseSystem : BasicSingleton<ShowcaseSystem>
{
    [SerializeField] Vector3 sceneOffset = new Vector3(0, -50, 50);
    [SerializeField] Vector2 heightLimits;

    [SerializeField] float rotateSpeed;
    [SerializeField] float translateSpeed;
    [SerializeField] float stopTime = 1;
    
    [SerializeField] Transform cameraPivot;
    [SerializeField] Cinemachine.CinemachineVirtualCamera showcaseCam;
    [SerializeField] Skybox skybox;

    Dictionary<UnityEngine.AddressableAssets.AssetReference, UnityEngine.Object> loadedAssets = new Dictionary<UnityEngine.AddressableAssets.AssetReference, UnityEngine.Object>();

    Vector3 lastMousePos;
    Vector2 velocity;

    private void Update()
    {
        if (!showcaseCam.enabled) return;

        if (Input.GetMouseButtonDown(0)) DOTween.Kill(GetInstanceID());

        if (Input.GetMouseButton(0))
        {
            var diff = lastMousePos - Input.mousePosition;
            var dir = diff.normalized;
            var delta = diff.magnitude;
            
            velocity = new Vector2(Mathf.Abs(delta * dir.x), Mathf.Abs(delta * dir.y));

            if (Input.mousePosition.x - lastMousePos.x < 0) velocity.x *= -1;
            if (lastMousePos.y - Input.mousePosition.y < 0) velocity.y *= -1;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            DOTween.To(() => velocity, (x) => velocity = x, Vector2.zero, stopTime).SetId(GetInstanceID());
        }

        cameraPivot.localEulerAngles += new Vector3(0, rotateSpeed * velocity.x * Time.deltaTime, 0);
        showcaseCam.transform.localPosition = new Vector3(
            showcaseCam.transform.localPosition.x,
            Mathf.Clamp(showcaseCam.transform.localPosition.y + translateSpeed * velocity.y * Time.deltaTime, heightLimits.x, heightLimits.y),
            showcaseCam.transform.localPosition.z);
    }

    private void LateUpdate()
    {
        lastMousePos = Input.mousePosition;
    }

    public void ShowcaseCharacter(CharacterObject character)
    {
        StartCoroutine(LoadRoutine(character));
    }

    CharacterObject loadedCharacter;
    GameObject loadedEnvironment;
    Material loadedSkybox;
    GameObject loadedRig;

    IEnumerator LoadRoutine(CharacterObject character)
    {
        float startTime = Time.time;

        loadedCharacter = character;
        loadedEnvironment = null;
        loadedSkybox = null;
        loadedRig = null;

        bool alreadyLoaded = true;

        {
            var asset = character.characterRig;
            AsyncOperationHandle<GameObject> loadOp;
            if (!asset.OperationHandle.IsValid())
            {
                loadOp = asset.LoadAssetAsync<GameObject>();
                loadOp.Completed += CharacterLoaded;
                alreadyLoaded = false;
            }
            else
            {
                loadedRig = loadedAssets[asset] as GameObject;
                loadedRig.SetActive(true);
            }
        }

        {
            var asset = character.environmentAsset;
            AsyncOperationHandle<GameObject> loadOp;
            if (!asset.OperationHandle.IsValid())
            {
                loadOp = asset.LoadAssetAsync<GameObject>();
                loadOp.Completed += EnvironmentLoaded;
                alreadyLoaded = false;
            }
            else
            {
                loadedEnvironment = loadedAssets[asset] as GameObject;
                loadedEnvironment.SetActive(true);
            }
        }

        {
            var asset = character.skyboxAsset;
            AsyncOperationHandle<Material> loadOp;
            if (!asset.OperationHandle.IsValid())
            {
                loadOp = asset.LoadAssetAsync<Material>();
                loadOp.Completed += SkyboxLoaded;
                alreadyLoaded = false;
            }
            else
            {
                loadedSkybox = loadedAssets[asset] as Material;
                skybox.material = loadedSkybox;
            }
        }

        if (!alreadyLoaded)
        {
            screenEffects.ShowLoadingIcon(0);
            screenEffects.BlackOut(ScreenEffects.EffectType.Partial);
        }

        characterSidebar.Canvas.Show();
        showcaseCam.enabled = true;

        yield return new WaitUntil(() => loadedEnvironment && loadedSkybox && loadedRig);

        // Has the camera finished moving?
        yield return new WaitUntil(() => Time.time - startTime >= mapSceneManager.CinemachineBrain.m_DefaultBlend.BlendTime);

        if (!alreadyLoaded)
        {
            screenEffects.FadeFromBlack(ScreenEffects.EffectType.Partial, 0.5f);
            screenEffects.HideLoadingIcon(0.5f);
        }

        loadedAssets[character.environmentAsset] = loadedEnvironment;
        loadedAssets[character.skyboxAsset] = loadedSkybox;
        loadedAssets[character.characterRig] = loadedRig;
    }

    private void EnvironmentLoaded(AsyncOperationHandle<GameObject> obj)
    {
        loadedEnvironment = Instantiate(obj.Result, sceneOffset, obj.Result.transform.rotation);
    }

    private void SkyboxLoaded(AsyncOperationHandle<Material> obj)
    {
        skybox.material = obj.Result;
        loadedSkybox = obj.Result;
    }

    private void CharacterLoaded(AsyncOperationHandle<GameObject> obj)
    {
        loadedRig = Instantiate(obj.Result);
        loadedRig.transform.position = sceneOffset;
        loadedRig.transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void HideShowcase()
    {
        if (loadedCharacter)
        {
            (loadedAssets[loadedCharacter.characterRig] as GameObject).SetActive(false);
            (loadedAssets[loadedCharacter.environmentAsset] as GameObject).SetActive(false);
            showcaseCam.enabled = false;
        }
    }

    public void Cleanup()
    {
        foreach (var item in loadedAssets.Keys)
        {
            item.ReleaseAsset();
        }
        loadedAssets.Clear();
    }
}