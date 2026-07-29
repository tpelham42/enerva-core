using UnityEngine;
using System.Collections;

namespace EnervaCore.YieldRoutines {    

    public class YTWaitForSeconds : YieldTask {
        float returnTime;
        bool useRawTime;

        public YTWaitForSeconds(float timeInSeconds, bool UseRawTime = false) {
            useRawTime = UseRawTime;

            if (useRawTime) {
                returnTime = Time.time + timeInSeconds;
            }
            else {
                returnTime = Time.time + timeInSeconds;//GameManager.Instance.TimeMgr.TotalTimeSeconds + timeInSeconds;
            }
        }

        public override IEnumerator Run() {
            if (useRawTime) {
                while (Time.time < returnTime) {
                    yield return null;
                }
            }
            else {
                while (Time.time < returnTime) {
                    yield return null;
                }
                /*while (GameManager.Instance.TimeMgr.TotalTimeSeconds < returnTime) {
                    yield return null;
                }*/
            }
        }
    }
}
