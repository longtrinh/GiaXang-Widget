namespace PetrolimexWidget
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "{PETROLIMEX-WIDGET-UNIQUE-ID-123456}");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                mutex.ReleaseMutex();
            }
            else
            {
                Application.Exit();
            }
        }
    }
}