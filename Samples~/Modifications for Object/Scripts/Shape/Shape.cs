using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// The context that modifications configure. Wraps a 2D UI Image; each modification
    /// in the list is applied to a Shape instance to tweak its color, name, transform, etc.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Shape : MonoBehaviour
    {
        [SerializeField] private Image targetImage;

        public Image Image => targetImage ? targetImage : (targetImage = GetComponent<Image>());

        private void Reset() => targetImage = GetComponent<Image>();

        public void SetColor(Color color)
        {
            if (Image)
                Image.color = color;
        }
    }
}