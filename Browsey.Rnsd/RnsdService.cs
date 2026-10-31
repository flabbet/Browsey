using System.Diagnostics;

namespace Browsey.Rnsd;

public class RnsdService
{
    public event Action<string> OnPageFetched;
    public void Start()
    {
        //Thread rnsThread = new Thread(() =>
        //{
            RunBridge();
        //});

        //rnsThread.Start();
    }

    private void RunBridge()
    {
        var python = Path.GetFullPath("./rns-bridge/Scripts/python.exe");
        var script = Path.GetFullPath("./rns-bridge/src/bridge.py");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = python,
                WorkingDirectory = Path.GetDirectoryName(script)!,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
            }
        };

        process.StartInfo.ArgumentList.Add(script);

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                if (e.Data.StartsWith("[__PGDATA]:"))
                {
                    string page = e.Data.Substring("[__PGDATA]:".Length);
                    OnPageFetched?.Invoke(page);
                }
                Console.WriteLine($"[Python] {e.Data}");
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[Python Error] {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }
}