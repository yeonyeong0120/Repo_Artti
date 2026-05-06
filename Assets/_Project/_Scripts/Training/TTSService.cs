// TTSService.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Artti.Training
{
    public class TTSService : MonoBehaviour
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject tts;
        private bool initialized = false;

        private class InitListener : AndroidJavaProxy
        {
            public System.Action<bool> onDone;
            public InitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }
            public void onInit(int status) => onDone?.Invoke(status == 0);
        }

        private void Awake()
        {
            var context = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");

            var listener = new InitListener();
            listener.onDone = success =>
            {
                if (!success)
                {
                    Debug.LogError("[TTS] 초기화 실패");
                    return;
                }
                var locale = new AndroidJavaObject("java.util.Locale", "ko", "KR");
                int result = tts.Call<int>("setLanguage", locale);
                if (result < 0)
                    Debug.LogWarning($"[TTS] 한국어 설정 실패 (결과코드: {result}) — 기기 한국어 TTS 팩 확인 필요");

                initialized = true;
                Debug.Log("[TTS] 초기화 완료 (한국어)");
            };

            tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", context, listener);
        }

        private void OnDestroy()
        {
            if (tts != null)
            {
                tts.Call("stop");
                tts.Call("shutdown");
            }
        }

        public async UniTask Speak(string text, CancellationToken ct)
        {
            if (!initialized || string.IsNullOrEmpty(text)) return;

            // QUEUE_FLUSH = 0 : 현재 재생 중인 것 중단하고 바로 재생
            tts.Call<int>("speak", text, 0, null, "artti_utt");

            // TTS 시작까지 짧게 대기 후 완료될 때까지 polling
            await UniTask.Delay(150, cancellationToken: ct);
            await UniTask.WaitUntil(() => !tts.Call<bool>("isSpeaking"), cancellationToken: ct);
        }
#else
        // 에디터/iOS에서는 로그로 대체
        private void Awake()
        {
            Debug.Log("[TTS] 에디터 모드 — 실제 음성 없음");
        }

        public UniTask Speak(string text, CancellationToken ct)
        {
            Debug.Log($"[TTS Mock] {text}");
            return UniTask.CompletedTask;
        }
#endif
    }
}
