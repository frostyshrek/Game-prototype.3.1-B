using UnityEngine;
using TMPro;

namespace BattleSystem
{
    public class WorldSpaceHPUI : MonoBehaviour
    {
        public Transform target;             // character to follow
        public TMP_Text hpText;              // assign HPText
        public Vector3 offset = new Vector3(0f, 2.3f, 0f);
        public Camera cam;                   // auto-fills to Camera.main if null

        void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        void LateUpdate()
        {
            if (!target || !cam) return;

            // follow
            transform.position = target.position + offset;

            // face camera (billboard)
            transform.forward = cam.transform.forward;
        }

        public void SetHP(int current, int max)
        {
            if (hpText) hpText.text = $"{current} / {max}";
        }
    }
}
