#if !UNITY_ANDROID
using System.Windows.Forms;
using System.IO;

public static class WindowsFileExplorer
{
    public static string OpenSaveImageDialog()
    {
        var filePath = string.Empty;

        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.InitialDirectory = UnityEngine.Application.dataPath;

            saveFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
            }
        }

        return filePath;
    }

    public static string OpenLoadImageDialog()
    {
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;

            openFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
        }

        return filePath;
    }

    /// <summary>
    /// Example borrowed from here
    /// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog?view=netframework-4.8
    /// </summary>
    public static string OpenTextBrowser()
    {
        var fileContent = string.Empty;
        var filePath = string.Empty;

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            // Set the initial directory to the game's folder location
            openFileDialog.InitialDirectory = UnityEngine.Application.dataPath;

            // Sorry sir, we're only accepting TXT files!
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    // Yeah just shovel all that text into my one string variable thank you
                    fileContent = reader.ReadToEnd();
                }
            }
        }

        return fileContent;
    }
}
#endif