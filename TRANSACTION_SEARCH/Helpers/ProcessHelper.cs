using System.Diagnostics;

namespace TRANSACTION_SEARCH.Helpers
{
    public static class ProcessHelper
    {
        private const string processName = @"C:\Program Files (x86)\Notepad++\notepad++.exe";

        public static void LoadFileToEditor(string fileName)
        {
            Process process = new Process();
            ProcessStartInfo procInfo = new ProcessStartInfo()
            {
                FileName = processName,
                Arguments = fileName,
            };

            process.StartInfo = procInfo;
            process.Start();

            if (process != null)
            {
                process.Dispose();
            }
        }
    }
}
