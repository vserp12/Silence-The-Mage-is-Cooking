using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Between-wave spell selection panel.
// All wiring is done here in code; the editor script GameSceneSetup creates the UI objects.
public class SpellSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Buttons")]
    public Button waterButton;
    public Button fireButton;
    public Button electricityButton;
    public Button lightButton;

    [Header("Level Labels (TMP, placed under each button)")]
    public TextMeshProUGUI waterLevelText;
    public TextMeshProUGUI fireLevelText;
    public TextMeshProUGUI electricityLevelText;
    public TextMeshProUGUI lightLevelText;

    [Header("Header")]
    public TextMeshProUGUI waveLabel;

    void Start()
    {
        waterButton?.onClick.AddListener(() => Pick(ElementType.Water));
        fireButton?.onClick.AddListener(() => Pick(ElementType.Fire));
        electricityButton?.onClick.AddListener(() => Pick(ElementType.Electricity));
        lightButton?.onClick.AddListener(() => Pick(ElementType.Light));
        Hide();
    }

    public void Show(int completedWaves)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        if (waveLabel != null)
        {
            waveLabel.text = completedWaves == 0
                ? "Choose your starting spell!"
                : $"Wave {completedWaves} cleared!\nChoose a spell to upgrade or learn:";
        }

        RefreshLevelLabels();
    }

    public void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Pick(ElementType element)
    {
        int newLevel = PlayerSpellInventory.Instance.SelectElement(element);

        var caster = FindObjectOfType<SpellCaster>();
        if (caster != null) caster.SetSpellByElement(element, newLevel);

        WaveManager.Instance.OnSpellSelected();
    }

    void RefreshLevelLabels()
    {
        SetLabel(waterLevelText, ElementType.Water);
        SetLabel(fireLevelText, ElementType.Fire);
        SetLabel(electricityLevelText, ElementType.Electricity);
        SetLabel(lightLevelText, ElementType.Light);
    }

    void SetLabel(TextMeshProUGUI tmp, ElementType elem)
    {
        if (tmp == null || PlayerSpellInventory.Instance == null) return;
        int lvl = PlayerSpellInventory.Instance.GetLevel(elem);
        tmp.text = lvl == 0 ? "NEW" : $"Lv.{lvl}";
    }
}
