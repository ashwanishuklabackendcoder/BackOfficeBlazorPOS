using BackOfficeBlazor.Admin.Services.Interfaces;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ZplBuilder : IZplBuilder
    {
        public string BuildTestLabel()
        {
            return "^XA^CF0,30^FO30,40^FDCloud POS Test Label^FS^CF0,24^FO30,90^FDZPL connectivity OK^FS^FO30,140^FDUTC:^FS^FO110,140^FD" +
                   DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") +
                   "^FS^XZ";
        }
    }
}
