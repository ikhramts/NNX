using System;
using ExcelDna.Integration;

namespace NNX
{
    public class AddIn : IExcelAddIn
    {
        public void AutoOpen()
        {
            ExcelIntegration.RegisterUnhandledExceptionHandler(ex => $"[Error] {((Exception)ex).Message}");
        }

        public void AutoClose()
        {
        }
    }
}
