namespace youtube
{
    public static class Program
    {
        // public static property to hold the single main Form1 instance
        public static Form1? MainForm { get; set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Setup.Man();

            MainForm = new Form1();
            Application.Run(MainForm);
        }
    }
}