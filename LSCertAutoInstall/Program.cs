using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace CertRequestor
{
   static class Program
   {
      [STAThread]
      static void Main()
      {
          using (Mutex mutex = new Mutex(false, @"Global\99557e68-e032-4a0a-99e0-c5ce14dd5015"))
          {
              if (!mutex.WaitOne(0, false))
              {
                  MessageBox.Show(null, "Приложение уже было запущено ранее дождитесь его выполнения и повторите попытку.", "Пролижение уже запущено.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  return;
              }

              GC.Collect();

              Application.EnableVisualStyles();
              Application.SetCompatibleTextRenderingDefault(false);
              Application.Run(new LinkCRequest.CertAutoInstall_form());
          }
      }
   }
}
