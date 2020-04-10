using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SampleBehaviour : MonoBehaviour
{
    static readonly int TextureArray = Shader.PropertyToID("_TextureArray");

    [SerializeField] Button _buttonPrefab = null;
    [SerializeField] Texture2D[] _textures = null;

    MeshGenerator _meshGenerator;
    byte[,] _bytes;
    byte _byteToDraw = 0;
    Button[] _buttons;

    void Start()
    {
        _meshGenerator = new MeshGenerator();
        _bytes = GetRandomMap(32, 16, _textures.Length);
        UpdateMesh();
        GetComponent<MeshRenderer>().material.SetTexture(TextureArray, CreateTextureArray(_textures));

        _buttons = new Button[_textures.Length];
        for(var i = 0; i < _textures.Length; i ++)
        {
            var btn = GameObject.Instantiate(_buttonPrefab.gameObject, _buttonPrefab.transform.parent).GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = _textures[i].name;
            var drawByte = (byte) i;
            _buttons[i] = btn;
            btn.onClick.AddListener(() => 
            {
                EnableAllButtons();
                btn.interactable = false;
                _byteToDraw = drawByte;
            });
        }
        _buttons[0].interactable = false;
        _buttonPrefab.gameObject.SetActive(false);



        Camera.main.orthographicSize = 960f / ((( 960f / 540f ) * 2f ) * 128f );
    }

    void EnableAllButtons()
    {
        foreach(var btn in _buttons) btn.interactable = true;
    }

    void UpdateMesh()
    {
        var mesh = _meshGenerator.Generate(_bytes);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
        {
            var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryDraw(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y), _byteToDraw);
        }
    }

    void TryDraw(int x, int y, byte value)
    {
        x = Mathf.Clamp(x, 0, _bytes.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, _bytes.GetLength(1) - 1);
        _bytes[x, y] = value;
        UpdateMesh();
    }

    static byte[,] GetRandomMap(int width, int height, int textureCount)
    {
        var result = new byte[width, height];
        for(var x = 0; x < width; x ++)
        {
            for(var y = 0; y < height; y ++)
            {
                result[x, y] = (byte) Random.Range(0, textureCount);
            }
        }
        return result;
    }

    static Texture2DArray CreateTextureArray(Texture2D[] textures)
    {
        var t = textures[0];
        var textureArray = new Texture2DArray(t.width, t.height, textures.Length, t.format, t.mipmapCount > 1, false);
        textureArray.anisoLevel = t.anisoLevel;
        textureArray.filterMode = t.filterMode;
        textureArray.wrapMode = t.wrapMode;
        for (var i = 0; i < textures.Length; i++)
        {
            for (var mipmap = 0; mipmap < textures[i].mipmapCount; mipmap++)
            {
                Graphics.CopyTexture(textures[i], 0, mipmap, textureArray, i, mipmap);
            }
        }
        textureArray.Apply(false);
        return textureArray;
    }
}
