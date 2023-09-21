using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RandomTexture : MonoBehaviour
{

    public GameObject[] walls;
    //private Renderer renderer;
    //public Material floorMaterial;
    //public Texture2D floorTexture;
    int index;

    public float texChangeTime = .2f;
    public float changeDelay = 0f;


    DirectoryInfo di = new DirectoryInfo("Assets/Resources/dtd/images/");
    DirectoryInfo[] texture_dir_list;

    // Start is called before the first frame update
    void Start()
    {
        texture_dir_list = di.GetDirectories();

        InvokeRepeating("LoadRoomTextures", changeDelay, texChangeTime);
        
        //Renderer floorRenderer = floor.transform.GetComponent<Renderer>();
        //LoadRandomTextures(floorRenderer);



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadRoomTextures()
    {

        Renderer[] allChildRenderers = gameObject.transform.GetComponentsInChildren<Renderer>();
        //Transform currentTransform = gameObject.transform;

        foreach ( Renderer childRenderer in allChildRenderers)
        {
            //Debug.Log("Test message");
            LoadRandomTextures(childRenderer);
        }
        
    }


    void LoadRandomTextures(Renderer floorRenderer)
    {
        //Renderer floorRenderer = floor.transform.GetComponent<Renderer>();


        index = Random.Range(0, texture_dir_list.Length);
        string tex_type = texture_dir_list[index].Name;

        FileInfo[] texture_list = texture_dir_list[index].GetFiles("*.jpg");
        index = Random.Range(0, texture_list.Length);
        string tex_name = Path.GetFileNameWithoutExtension(texture_list[index].Name);
        //string extension = Path.GetFileNameWithoutExtension();//texture_list[index].Extension;
        


        string path = "dtd/images/" + tex_type + '/'+tex_name; //"flecked/flecked_0003"
        //string extension = Path.GetFileNameWithoutExtension(path+".jpg");

        //Debug.Log(path);

        //LoadTextureFromPath("candies", floorRenderer);
        LoadTextureFromPath(path, floorRenderer);

    }

    // path is relative to resources folder
    void LoadTextureFromPath(string path, Renderer renderer)
    {
        Texture2D texture = Resources.Load<Texture2D>(path);
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = texture;
        renderer.material = material;

    }

}
