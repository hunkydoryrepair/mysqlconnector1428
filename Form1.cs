using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace MySqlConnectorWinForms1428
{
    public partial class Form1 : Form
    {
        private String results = "";
        public Form1()
        {
            InitializeComponent();
        }

        private async Task DoSomeSync()
        {
            using (var conn = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=plposdev;UID=devpos;Pwd=happy123;Charset=utf8mb4;ConnectionReset=true;maxpoolsize=1"))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                results += String.Format("After OpenAsync: {0}, Thread {1}\n", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MySqlConnection.ClearAllPools(); // empty all connection pools
            results = String.Format("Initial Context {0}: Thread {1}\n", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
            richTextBox1.Text = results;

            DoSomeSync().Wait(); // tie up our UI thread while we wait.

            richTextBox1.Text = results;

            Task.Run(async () =>
            {
                // new context
                results += String.Format("Task.Run Context {0} (always null), Thread {1}\n", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
                using (var conn = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=plposdev;UID=devpos;Pwd=happy123;Charset=utf8mb4;ConnectionReset=true;maxpoolsize=1"))
                {
                    await conn.OpenAsync();
                    if (SynchronizationContext.Current != null)
                    {
                        results += "ERROR! Should not have SynchronizationContext set!\n";
                    }
                    results += String.Format("After OpenAsync: {0}, Thread: {1}\n", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
                }
                // on UI thread, set the text
                this.Invoke(new MethodInvoker(delegate
                {
                    richTextBox1.Text = results;
                }));
            });
        }

    }
}
