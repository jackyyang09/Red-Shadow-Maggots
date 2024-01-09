using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.Win32;
using System.Runtime.InteropServices;
using System;
using RSMConstants;

[Serializable]
public struct CharacterCard
{
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    //public string name;
    public Rarity rarity;
    public byte characterID;
    public long time;
}
//
// https://www.facebook.com/note.php?note_id=437497056995
// https://gamedev.stackexchange.com/questions/72760/how-can-i-store-game-metadata-in-a-png-file
// 
public class CardLoader : MonoBehaviour
{
    [SerializeField] List<CharacterObject> characters = null;

    [Header("Object References")]

    [SerializeField]
    Camera cam = null;

    [SerializeField] CharacterCardHolder exportCardHolder = null;

    [SerializeField] OptimizedCanvas imageDropOverlay = null;

    [SerializeField] TMPro.TextMeshProUGUI idText = null;

    [Header("TEST DATA TO BE DELETED")]

    [SerializeField] LayerMask cardLayers = new LayerMask();

    [SerializeField] public CharacterCard testData = new CharacterCard();
    [SerializeField] byte[] testDataStorage = null;

    [SerializeField] string testIDinput = null;

    [SerializeField] string imageFilePath = null;

    [SerializeField] Texture2D savedImage = null;
    [SerializeField] RenderTexture renderTexture = null;

    public static CardLoader instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //
    //}

    private void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFileDropped;
    }
    
    private void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    // Update is called once per frame
    //void Update()
    //{
    //
    //}

    public CharacterObject CharacterIDToObject(int id)
    {
        return characters[id];
    }

    public void ShowFileDropOverlay()
    {
        imageDropOverlay.Show();
    }

    public void HideFileDropOverlay()
    {
        imageDropOverlay.Hide();
    }

    public void ImportMaggotFromBrowseButton()
    {
#if !UNITY_ANDROID
        string filePath = WindowsFileExplorer.OpenLoadImageDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            Texture2D tex = ReadTexture2DFromFilePath(filePath);
            var newCharacter = GetCharacterCardFromTexture(tex);

            UncrateNewMaggot(newCharacter.characterID, newCharacter.rarity);
        }
#endif
    }

    private void OnFileDropped(List<string> filePaths, POINT dropPoint)
    {
        string file = "";
        // scan through dropped files and filter out supported image types
        for (int i = 0; i < filePaths.Count; i++)
        {
            var fi = new System.IO.FileInfo(filePaths[i]);
            var ext = fi.Extension.ToLower();
            if (ext == ".png")
            {
                file = filePaths[i];
                break;
            }
        }

        if (!string.IsNullOrEmpty(file))
        {
            Texture2D tex = ReadTexture2DFromFilePath(file);
            var newCharacter = GetCharacterCardFromTexture(tex);
            if (!imageDropOverlay.IsVisible)
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(new Vector2(dropPoint.x, dropPoint.y));
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, cardLayers))
                {
                    var selectedCard = hit.rigidbody.GetComponent<CharacterCardHolder>();
                    selectedCard.SetCharacterAndRarity(characters[newCharacter.characterID], newCharacter.rarity);

                    UncrateNewMaggot(newCharacter.characterID, newCharacter.rarity);
                }
            }
            else
            {
                UncrateNewMaggot(newCharacter.characterID, newCharacter.rarity);
            }
        }
    }

    public void UncrateNewMaggot(int id, Rarity rarity)
    {
        imageDropOverlay.SetActive(false);
        CardMenuUI.selectedHolder.SetCharacterAndRarity(characters[id], rarity);
        UncrateSequence.Instance.UncrateCharacter(characters[id], rarity);
    }

    public void ExportCharacter(CharacterCardHolder cardHolder)
    {
        if (IsInvoking("CaptureExportCard")) return;
        // Retrieve data to export
        CharacterCard newData = new CharacterCard();
        newData.characterID = (byte)characters.IndexOf(cardHolder.Character);
        newData.rarity = cardHolder.Rarity;

        exportCardHolder.SetCharacterAndRarity(cardHolder.Character, cardHolder.Rarity);

        // Debug: Serialize data in inspector
        testDataStorage = SaveAsBytes(newData);

        idText.text = Convert.ToBase64String(testDataStorage);

        //CaptureExportCard();
        Invoke("CaptureExportCard", 0.025f);
    }

    /// <summary>
    /// Invoke this after 0.025s to get past Render Texture updates
    /// </summary>
    public void CaptureExportCard()
    {
        Texture2D tex = SerializeBytesToTexture2D(testDataStorage);

#if !UNITY_ANDROID
        string filePath = WindowsFileExplorer.OpenSaveImageDialog();
        if (!filePath.IsNullEmptyOrWhiteSpace())
        {
            SaveTextureAsPNG(tex, filePath);
        }
#endif
    }

    Texture2D ReadTexture2DFromFilePath(string filePath)
    {
        var data = System.IO.File.ReadAllBytes(filePath);
        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        tex.LoadImage(data);
        return tex;
    }

    CharacterCard GetCharacterCardFromTexture(Texture2D tex)
    {
        List<bool> bits = new List<bool>();

        Color32[] pixels = tex.GetPixels32();

        bool exit = false;
        int i = 0;
        int index = (BitConverter.IsLittleEndian) ? 0 : 31;
        for (int y = 0; y < tex.height && !exit; y++)
        {
            for (int x = 0; x < tex.width && !exit; x++)
            {
                Color32 refColor = pixels[i];

                var redBits = ((int)(refColor.r)).ToBinary();

                bits.Add(redBits[index]);

                if (refColor.a == 254) // We've reached the end
                {
                    exit = true;
                }
                i++;
            }
        }

        BitArray bitData = new BitArray(bits.ToArray());

        byte[] data = bitData.ToBytes();

        return LoadFromBytes<CharacterCard>(data);
    }

    [ContextMenu("Test Update CharacterHolder")]
    public void TestUpdateCharacterHolder()
    {
        exportCardHolder.SetCharacterAndRarity(characters[testData.characterID], testData.rarity);
    }

    [ContextMenu("Test Save Metadata")]
    public void TestSaveMetaData()
    {
        testDataStorage = SaveAsBytes(testData);
        
        idText.text = Convert.ToBase64String(testDataStorage);

        SaveTextureAsPNG(SerializeBytesToTexture2D(testDataStorage), imageFilePath);
    }

    [ContextMenu("Test Load Metadata")]
    public void TestLoadMetaData()
    {
        testData = GetCharacterCardFromTexture(savedImage);
    }

    [ContextMenu("Test Load ID")]
    public void TestLoadID()
    {
        var bytes = Convert.FromBase64String(testIDinput);
        foreach (byte b in bytes)
        {
            print(b);
        }
        testData = LoadFromBytes<CharacterCard>(bytes);
    }

    /// <summary>
    /// RenderTexture to Texture2D
    /// https://answers.unity.com/questions/9969/convert-a-rendertexture-to-a-texture2d.html
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    Texture2D SerializeBytesToTexture2D(byte[] bytes)
    {
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        tex.Apply();
        RenderTexture.active = null;

        //Texture2D referenceTex = new Texture2D(image.width, image.height);
        Texture2D referenceTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        referenceTex.SetPixels(tex.GetPixels());
        referenceTex.Apply();

        BitArray bits = new BitArray(bytes);

        int i = 0;
        int index = (BitConverter.IsLittleEndian) ? 0 : 31;
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color32 refColor = referenceTex.GetPixel(x, y);

                var redBits = ((int)(refColor.r)).ToBinary();
                var greenBits = ((int)(refColor.g)).ToBinary();
                var blueBits = ((int)(refColor.b)).ToBinary();

                redBits.Set(index, bits[i]);
                greenBits.Set(index, bits[i]);
                blueBits.Set(index, bits[i]);

                refColor.r = (byte)redBits.ToNumeral();
                refColor.g = (byte)greenBits.ToNumeral();
                refColor.b = (byte)blueBits.ToNumeral();

                tex.SetPixel(x, y, refColor);

                i++;
                if (i == bits.Length)
                {
                    refColor.a = 254;
                    tex.SetPixel(x, y, refColor);
                    tex.Apply();
                    return tex;
                }
            }
        }

        tex.Apply();
        return tex;
    }

    /// <summary>
    /// https://answers.unity.com/questions/1331297/how-to-save-a-texture2d-into-a-png.html
    /// </summary>
    /// <param name="_texture"></param>
    /// <param name="_fullPath"></param>
    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    public static byte[] SaveAsBytes<T>(T str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    public static T LoadFromBytes<T>(byte[] arr) where T : struct
    {
        T str = default(T);

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        str = (T)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}

public static class BinaryConverter
{
    public static BitArray ToBinary(this int numeral)
    {
        return new BitArray(new[] { numeral });
    }

    public static int ToNumeral(this BitArray binary)
    {
        if (binary == null)
            throw new ArgumentNullException("binary");
        if (binary.Length > 32)
            throw new ArgumentException("must be at most 32 bits long");

        var result = new int[1];
        binary.CopyTo(result, 0);
        return result[0];
    }

    /// <summary>
    /// Read a BitArray
    /// http://geekswithblogs.net/dbrown/archive/2009/04/05/convert-a-bitarray-to-byte-in-c.aspx
    /// </summary>
    /// <param name="bits"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this BitArray bits)
    {
        // calculate the number of bytes
        int numBytes = bits.Count / 8;

        // add an extra byte if the bit-count is not divisible by 8
        if (bits.Count % 8 != 0) numBytes++;

        // reserve the correct number of bytes
        byte[] bytes = new byte[numBytes];

        // get the 4 bytes that make up the 32 bit integer of the bitcount
        var prefix = BitConverter.GetBytes(bits.Count);

        // copy the bit-array into the byte array
        bits.CopyTo(bytes, 0);

        byte[] newArray = new byte[bytes.Length];

        int j = 0;

        for (int i = 0; i < bytes.Length; i++, j++)
        {
            newArray[j] = bytes[i];
        }

        return newArray;
    }
}