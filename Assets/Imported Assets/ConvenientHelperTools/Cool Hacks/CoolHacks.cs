using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CoolHacks : MonoBehaviour
{
    [System.Serializable]
    public struct Hack
    {
        public string hackText;
        public KeyCode keybind;
        public UnityEngine.Events.UnityEvent command;
    }

    [Header("Keybinds")]

    [SerializeField]
    KeyCode hackInput = KeyCode.Tilde;

    [SerializeField]
    List<Hack> hacks = new List<Hack>();

    [Header("Object References")]

    [SerializeField]
    Canvas hackerCanvas = null;

    [SerializeField]
    Text hackLogger = null;

    public static bool hacksEnabled = false;
    public static CoolHacks instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        hackLogger.enabled = true;
        hackLogger.CrossFadeAlpha(0, 0.01f, false);

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(hackInput))
        {
            hacksEnabled = !hacksEnabled;
            print("<color=green>Hacks Enabled: " + hacksEnabled + "</color>");
        }

        if (hacksEnabled)
        {
            for (int i = 0; i < hacks.Count; i++)
            {
                if (Input.GetKeyDown(hacks[i].keybind))
                {
                    InvokeHack(i);
                }
            }
        }
    }

    public void InvokeHack(int index)
    {
        hacks[index].command.Invoke();
        LogHack(hacks[index].hackText);
    }

    /// <summary>
    /// Displays text on screen
    /// Use this to show what hack you activated
    /// </summary>
    /// <param name="text"></param>
    public void LogHack(string text = "HACKED!")
    {
        hackLogger.CrossFadeAlpha(255, 0.01f, false);
        hackLogger.text = text;
        hackLogger.CrossFadeAlpha(0, 1f, false);
    }

    public void SpeedHack() => Time.timeScale = 2;
    public void TimeSlow() => Time.timeScale = 0.5f;

    private void Reset()
    {
        if (!hackerCanvas)
        {
            Debug.Log("Uh");

            hackerCanvas = GetComponent<Canvas>();
            hackerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hackerCanvas.sortingOrder = 100;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            Hack speedHack = new Hack();
            speedHack.hackText = "Speed Hax!";
            speedHack.keybind = KeyCode.Equals;
            speedHack.command = new UnityEngine.Events.UnityEvent();
            speedHack.command.AddListener(SpeedHack);
            hacks.Add(speedHack);

            Hack timeSlow = new Hack();
            timeSlow.hackText = "Time Slow!";
            timeSlow.keybind = KeyCode.Minus;
            timeSlow.command = new UnityEngine.Events.UnityEvent();
            timeSlow.command.AddListener(TimeSlow);
            hacks.Add(timeSlow);

            Hack timeNormal = new Hack();
            timeNormal.hackText = "Time Flows Again!";
            timeNormal.keybind = KeyCode.Backspace;
            timeNormal.command = new UnityEngine.Events.UnityEvent();
            timeNormal.command.AddListener(TimeSlow);
            hacks.Add(timeNormal);
        }
    }
}
