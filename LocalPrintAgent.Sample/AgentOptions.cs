namespace LocalPrintAgent.Sample
{
    public class AgentOptions
    {
        public string ApiBaseUrl { get; set; } = "http://localhost:5101";
        public string AgentKey { get; set; } = "CHANGE_ME";
        public string TerminalCode { get; set; } = "T01";
        public string LocationCode { get; set; } = "01";
        public string FileOutputDirectory { get; set; } = "C:\\Temp\\PosPrint";
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 25;
        public bool SmtpEnableSsl { get; set; } = false;
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public string EmailFrom { get; set; } = "noreply@local-pos";
    }
}
