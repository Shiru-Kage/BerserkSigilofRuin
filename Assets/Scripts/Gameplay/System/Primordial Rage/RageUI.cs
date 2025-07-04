using UnityEngine;
using UnityEngine.UI;

public class RageUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RageSystem rageSystem;
    [SerializeField] private Image rageFillImage;

    [Header("Smooth Fill")]
    [SerializeField] private float fillSpeed = 5f;

    private void Start()
    {
        if (rageFillImage == null)
            Debug.LogError("RageFillImage reference missing!");

        if (rageSystem == null)
            Debug.LogError("RageSystem reference missing!");
    }

    private void Update()
    {
        float targetFill = rageSystem.CurrentRage / rageSystem.MaxRage;
        rageFillImage.fillAmount = Mathf.Lerp(rageFillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
    }
}
