using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Base.Ads
{
    public class AdsNotice : MonoBehaviour
    {
        [SerializeField]
        protected UIAnimation anim = null;
        [SerializeField]
        protected Text message = null;

        protected int instanceId = 0;

        private void Awake()
        {
            instanceId = GetInstanceID();
        }

        private void Start()
        {
            Transform parent = UIAnimManager.SafeAreaTransform;
            if (parent == null)
                parent = UIAnimManager.RootTransform;
            if (parent != null)
                transform.SetParent(parent);
        }

        public void Show(string message = "Time to show ads... Please wait!", float timeOut = 2.5f)
        {
            if (anim == null)
                anim = GetComponent<UIAnimation>();
            if (anim != null)
            {
                DOTween.Kill(instanceId, false);
                this.message.text = message;
                anim.Show();
                DOVirtual.DelayedCall(timeOut, () =>
                {
                    anim.Hide(null);
                }, true).SetId(instanceId);
            }
        }

        public void Hide()
        {
            DOTween.Kill(instanceId, false);
            anim.Hide(null);
        }
    }
}
