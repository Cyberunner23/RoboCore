using RoboCore.Messages;

namespace DevTest
{
    public class TestMessage : MessageBase
    {
        private int _thing;
        public int Thing
        {
            get { return _thing;}
            set { _thing = value; }
        }
        
        public string Thing2 { get; set; }
        private float Thing3 { get; set; }
    }
}