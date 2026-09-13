using UnityEngine;

namespace Game.Combat
{
    public class CombatDebugHUD : MonoBehaviour
    {
        private ActionSystem _system;
        private Texture2D _tex;

        [Header("Runtime")] public bool isPlaying = true;

        private void Awake()
        {
            _system = GetComponent<ActionSystem>();
            // texture trắng 1x1 để tint màu
            _tex = new Texture2D(1, 1);
            _tex.SetPixel(0, 0, Color.white);
            _tex.Apply();
        }

        private void OnGUI()
        {
            Time.timeScale = GUI.HorizontalSlider(new Rect(50, 100, 200, 20), Time.timeScale, 0f, 1f);
            GUI.Label(new Rect(260, 100, 100, 20), $"TimeScale: {Time.timeScale:F2}");
            
            
            if (_system.CurrentRuntime == null) return;
            var data = _system.CurrentRuntime.Data;

            if (data == null) return;
            

            Rect bar = new Rect(800, 700, 400, 30);

            float total = data.timeStartUp + data.timeActive + data.timeRecovery;
            // Tỉ lệ
            float startupRatio = data.timeStartUp / total;
            float activeRatio = data.timeActive / total;
            float recoveryRatio = data.timeRecovery / total;

            // Width từng phase
            float startupW = bar.width * startupRatio;
            float activeW = bar.width * activeRatio;
            float recoveryW = bar.width * recoveryRatio;

            float x = bar.x;

            // 🔴 Startup
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(x, bar.y, startupW, bar.height), _tex);
            x += startupW;

            // 🟢 Active
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(x, bar.y, activeW, bar.height), _tex);
            x += activeW;

            // ⚪ Recovery (base xám)
            GUI.color = Color.gray;
            GUI.DrawTexture(new Rect(x, bar.y, recoveryW, bar.height), _tex);

            // 🟡 Cancel window (nằm trong recovery)
            float cancelStartRatio = data.cancelWindowStart / data.timeRecovery;
            float cancelEndRatio = data.cancelWindowEnd / data.timeRecovery;

            float cancelX = x + recoveryW * cancelStartRatio;
            float cancelW = recoveryW * (cancelEndRatio - cancelStartRatio);

            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(cancelX, bar.y, cancelW, bar.height), _tex);

            // ⚪ Marker theo thời gian
            float t = Mathf.Clamp01(_system.CurrentRuntime.ElapsedTime / total);
            float markerX = bar.x + bar.width * t;

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(markerX - 1, bar.y - 5, 2, bar.height + 10), _tex);

            // reset color
            GUI.color = Color.white;
        }
    }
}