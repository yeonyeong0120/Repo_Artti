using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Artti.Core
{
    /// <summary>
    /// Scene 전환 유틸리티.
    /// 비동기 로딩과 CancellationToken을 지원한다.
    /// </summary>
    public static class SceneLoader
    {
        public static async UniTask LoadSceneAsync(
            string sceneName,
            CancellationToken ct = default)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) return;

            // allowSceneActivation을 false로 두면
            // 로딩 진행률 UI를 붙일 수 있으나, MVP에서는 즉시 전환.
            await op.WithCancellation(ct);
        }
    }
}