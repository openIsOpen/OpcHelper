namespace Zlw.OpcClient
{
    public class OpcDaServer:OpcServer
    {
        private string _ProgId = "Kepware.KEPServerEX.V6";

        public string ProgId
        {
            get => _ProgId;
            set => _ProgId = value;
        }

        public int  ItemCountPerGroup  { get; set; }
    }
}
