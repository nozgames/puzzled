using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIColorPicker : MonoBehaviour
    {
        [SerializeField] private int _textureSize = 128;
        private Texture2D _textureHSV;
        private Texture2D _textureH;
        private Texture2D _textureS;
        private Texture2D _textureV;
        private Texture2D _textureA;

        [SerializeField] private RectTransform _popup = null;
        [SerializeField] private RectTransform _colorTransform = null;

        [SerializeField] private Button _previousButton = null;

        [SerializeField] private Image _previous = null;
        [SerializeField] private Image _current = null;

        [SerializeField] private UISlider _h = null;
        [SerializeField] private UISlider _s = null;
        [SerializeField] private UISlider _v = null;
        [SerializeField] private UISlider _a = null;

        [SerializeField] private RawImage _imageHSV = null;
        [SerializeField] private RawImage _imageH = null;
        [SerializeField] private RawImage _imageS = null;
        [SerializeField] private RawImage _imageV = null;
        [SerializeField] private RawImage _imageA = null;
        [SerializeField] private RawImage _imageCurrentBackground = null;
        [SerializeField] private RawImage _imagePreviousBackground = null;

        [SerializeField] private UIColorTexture _colorTexture = null;
        [SerializeField] private TMPro.TMP_InputField _hex = null;

        public System.Action<Color,bool> onValueChanged;

        public RectTransform popup => _popup;

        public Color value {
            get {
                var color = Color.HSVToRGB(_h.value / 100.0f, _s.value / 100.0f, _v.value / 100.0f);
                return new Color(color.r, color.g, color.b, _a.value / 100.0f);
            }
            set {
                _previous.color = value;
                _current.color = value;
                Color.RGBToHSV(value, out var h, out var s, out var v);
                _h.SetValueWithoutNotify(h * 100.0f);
                _s.SetValueWithoutNotify(s * 100.0f);
                _v.SetValueWithoutNotify(v * 100.0f);
                _a.SetValueWithoutNotify(value.a * 100.0f);

                if (!_hex.isFocused)
                    UpdateHex();
                    
                UpdateTextures();
            }
        }

        private void Awake()
        {
            _hex.onValueChanged.AddListener((v) => {
                if (ColorUtility.TryParseHtmlString($"#{v}", out var color))
                    value = color;
                else
                    value = Color.white;
            });

            _colorTexture.onValueChanged.AddListener((s, v) => {
                _s.value = s * 100.0f;
                _v.value = v * 100.0f;
                UpdateTextures();
                onValueChanged?.Invoke(value,false);
            });

            _previousButton.onClick.AddListener(() => {
                value = _previous.color;
                onValueChanged?.Invoke(value,false);
            });

            _h.onValueChanged.AddListener((v) => OnHSVChanged());
            _s.onValueChanged.AddListener((v) => OnHSVChanged());
            _v.onValueChanged.AddListener((v) => OnHSVChanged());
            _a.onValueChanged.AddListener(v => {
                _current.color = value;
                onValueChanged?.Invoke(value,false);
                UpdateHex();
            });

            UpdateTextureH();
            UpdateTextures();
            UpdateTextureA();
        }

        private void OnDisable()
        {
            onValueChanged?.Invoke(value, true);
            onValueChanged = null;
        }

        private void OnHSVChanged()
        {
            onValueChanged?.Invoke(value,false);
            UpdateTextures();
            UpdateHex();
        }

        private void UpdateTextures()
        {
            UpdateTextureS();
            UpdateTextureV();
            UpdateTextureHSV();

            _current.color = value;

            _colorTransform.anchorMin = _colorTransform.anchorMax =
                new Vector2(_s.value / 100.0f, _v.value / 100.0f);
        }

        private void UpdateTextureHSV ()
        {
            if (_textureHSV == null)
            {
                _textureHSV = new Texture2D(_textureSize, _textureSize);
                _textureHSV.wrapMode = TextureWrapMode.Clamp;
                _imageHSV.texture = _textureHSV;
            }

            for (var y = 0; y < _textureSize; y++)
                for (var x = 0; x < _textureSize; x++)
                    _textureHSV.SetPixel(x, y, Color.HSVToRGB(_h.value / 100.0f, x / (float)_textureSize, y / (float)_textureSize));

            _textureHSV.Apply();
        }

        private void UpdateTextureH()
        {
            if (_textureH == null)
            {
                _textureH = new Texture2D(_textureSize, 1);
                _textureH.wrapMode = TextureWrapMode.Clamp;
                _imageH.texture = _textureH;
            }

            for (var x = 0; x < _textureSize; x++)
                _textureH.SetPixel(x, 0, Color.HSVToRGB(x / (float)_textureSize, 1.0f, 1.0f));

            _textureH.Apply();
        }

        private void UpdateTextureS()
        {
            if (_textureS == null)
            {
                _textureS = new Texture2D(_textureSize, 1);
                _textureS.wrapMode = TextureWrapMode.Clamp;
                _imageS.texture = _textureS;
            }

            for (var x = 0; x < _textureSize; x++)
                _textureS.SetPixel(x, 0, Color.HSVToRGB(_h.value / 100.0f, x / (float)_textureSize, _v.value / 100.0f));

            _textureS.Apply();
        }

        private void UpdateTextureV()
        {
            if (_textureV == null)
            {
                _textureV = new Texture2D(_textureSize, 1);
                _textureV.wrapMode = TextureWrapMode.Clamp;
                _imageV.texture = _textureV;
            }

            for (var x = 0; x < _textureSize; x++)
                _textureV.SetPixel(x, 0, Color.HSVToRGB(_h.value / 100.0f, _s.value / 100.0f, x / (float)_textureSize));

            _textureV.Apply();
        }

        private void UpdateTextureA()
        {
            if (_textureA == null)
            {
                _textureA = new Texture2D(_textureSize, 32);
                _textureA.wrapMode = TextureWrapMode.Clamp;
                _imageA.texture = _textureA;
                _imageCurrentBackground.texture = _textureA;
                _imagePreviousBackground.texture = _textureA;
            }

            for (var y = 0; y < _textureSize; y++)
            for (var x = 0; x < _textureSize; x++)
                {
                    var grid = ((x / 8) + (y / 8)) % 2;
                    var a = x / (float)_textureSize;
                    _textureA.SetPixel(x, y, Color.Lerp(grid == 0 ? Color.white : new Color(.75f, .75f, .75f), Color.white, a));
                }

            _textureA.Apply();
        }

        private void UpdateHex()
        {
            _hex.SetTextWithoutNotify(ColorUtility.ToHtmlStringRGBA(value));
        }
    }
}
