﻿using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using static Facade;

namespace Map
{
    public class ScrollNonUI : BasicSingleton<ScrollNonUI>
    {
        [SerializeField] Vector2 xRange;
        [SerializeField] Vector2 zRange;
        [SerializeField] float sensitivity = 1;
        [SerializeField] float mouseReleaseSpeed;
        [SerializeField] float velocityDecay;

        Vector3 velocity;

        [SerializeField] new Camera camera;
        [SerializeField] Transform vCam;

        private bool dragging;

        Vector3 startPos;
        Vector3 mouseDownPos;

        Vector3 mouseVelocity 
        { 
            get 
            { 
                var mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
                return (lastMousePos - mousePos) * mouseReleaseSpeed / Time.fixedDeltaTime;
            }
        }
        Vector3 lastMousePos;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => PlayerSaveManager.Initialized);

            var pos = PlayerSaveManager.Instance.LoadedData.MapPosition;
            vCam.position = new Vector3(pos.x, vCam.position.y, pos.y);
        }

        private void OnApplicationQuit()
        {
            if (!PlayerSaveManager.Initialized) return;
            SaveMapPosition();
            playerDataManager.SaveData();
        }

        public void SaveMapPosition()
        {
            PlayerData.MapPosition = new Vector2(vCam.position.x, vCam.position.z);
        }

        public void OnMouseUp()
        {
            dragging = false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                dragging = true;
                mouseDownPos = camera.ScreenToViewportPoint(Input.mousePosition);
                startPos = vCam.position;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                velocity = new Vector3(mouseVelocity.x, 0, mouseVelocity.y);
                dragging = false;
            }

            if (dragging)
            {
                var mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
                var diff = mouseDownPos - mousePos;

                vCam.position = new Vector3(
                    Mathf.Clamp(startPos.x + diff.x / sensitivity, xRange.x, xRange.y),
                    vCam.position.y,
                    Mathf.Clamp(startPos.z + diff.y / sensitivity, zRange.x, zRange.y));
            }
            else
            {
                vCam.position += velocity * Time.fixedDeltaTime;
                vCam.position = new Vector3(
                    Mathf.Clamp(vCam.position.x, xRange.x, xRange.y),
                    vCam.position.y,
                    Mathf.Clamp(vCam.position.z, zRange.x, zRange.y));
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, velocityDecay * Time.fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            lastMousePos = camera.ScreenToViewportPoint(Input.mousePosition);
        }
    }
}