using System.Collections;
using TMPro;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
    private TextMeshProUGUI comboText;
    private Animator comboAnimator;

    [SerializeField] private float comboTime;
    [SerializeField] private float comboTimeReset;
    [SerializeField] private float comboTimeChange;
    [SerializeField] private int comboCount;

    private void Start()
    {
        comboText = GetComponent<TextMeshProUGUI>();
        comboAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        comboTime -= Time.deltaTime;
        ChangeComboText();

        if (comboTime <= 0f)
        {
            ResetCombo();
        }
    }

    private void ChangeComboText()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            comboCount++;
            comboText.text = $"Combo\n{comboCount}";
            comboTime = comboTimeChange;
            comboAnimator.SetBool("Appear", comboCount > 1);
            comboAnimator.SetBool("Disappear", false);
            StartCoroutine(ComboPop());
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboText.text = string.Empty;
        comboAnimator.SetBool("Appear", false);
        comboAnimator.SetBool("Disappear", true);
    }

    public void ResetDisappearAnimation()
    {
        comboAnimator.SetBool("Disappear", false);
    }

    private IEnumerator ComboPop()
    {
        if (comboCount < 2) yield break;

        comboAnimator.Play("ComboPop");
        yield return new WaitForSeconds(1f);
    }
}