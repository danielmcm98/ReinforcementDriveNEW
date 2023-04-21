using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class TimerUIController : MonoBehaviour
    {
        public TextMeshProUGUI timerText;

        public IEnumerator BeginCountdown()
        {
            // Countdown sequence
            timerText.text = "3";
            yield return new WaitForSeconds(1f);
            timerText.text = string.Empty;
            yield return new WaitForSeconds(.5f);

            timerText.text = "2";
            yield return new WaitForSeconds(1f);
            timerText.text = string.Empty;
            yield return new WaitForSeconds(.5f);

            timerText.text = "1";
            yield return new WaitForSeconds(1f);
            timerText.text = string.Empty;
            yield return new WaitForSeconds(.5f);

            timerText.text = "GO!";
            yield return new WaitForSeconds(1f);
            timerText.text = string.Empty;
        }
    }
}
