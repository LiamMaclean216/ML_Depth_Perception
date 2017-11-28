using UnityEngine;
using System.Collections;
using System.IO;


public class ScreenRecorder : MonoBehaviour
{

    public int captureWidth = 1920;
    public int captureHeight = 1080;

    public GameObject hideGameObject;

    public bool optimizeForManyScreenshots = true;

    public enum Format { RAW, JPG, PNG };
    public Format format = Format.PNG;

    public string folder;

    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;
    private int counter = 0;

    private bool captureScreenshot = false;
    private bool captureVideo = false;

    public Camera camera;

    private string uniqueFilename(int width, int height)
    {
        if (folder == null || folder.Length == 0)
        {
            folder = Application.dataPath;
            if (Application.isEditor)
            {
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += "/screenshots";

            System.IO.Directory.CreateDirectory(folder);

            string mask = string.Format("screen_{0}x{1}*.{2}", width, height, format.ToString().ToLower());
            counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
        }


        var filename = string.Format("{0}/screen_{1}x{2}_{3}" + Random.value + ".{4}", folder, width, height, counter, format.ToString().ToLower());

        ++counter;

        return filename;
    }


    void Update()
    {
        captureScreenshot |= Input.GetKeyDown("k");
        captureVideo = Input.GetKey("v");

        if (captureScreenshot)
        {
            captureScreenshot = false;
            TakeScreenShot(camera);
        }

    }

    public void TakeScreenShot(Camera camera)
    {
       

        if (hideGameObject != null) hideGameObject.SetActive(false);

        if (renderTexture == null)
        {

            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        }

        camera.targetTexture = renderTexture;
        camera.Render();


        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);

        camera.targetTexture = null;
        RenderTexture.active = null;

        string filename = uniqueFilename((int)rect.width, (int)rect.height);

        byte[] fileHeader = null;
        byte[] fileData = null;
        if (format == Format.RAW)
        {
            fileData = screenShot.GetRawTextureData();
        }
        else if (format == Format.PNG)
        {
            fileData = screenShot.EncodeToPNG();
        }
        else if (format == Format.JPG)
        {
            fileData = screenShot.EncodeToJPG();
        }
        else
        {
            string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = screenShot.GetRawTextureData();
        }

        new System.Threading.Thread(() =>
        {
            var f = System.IO.File.Create(filename);
            if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
        }).Start();

        if (hideGameObject != null) hideGameObject.SetActive(true);

        if (optimizeForManyScreenshots == false)
        {
            Destroy(renderTexture);
            renderTexture = null;
            screenShot = null;
        }

    }
}

