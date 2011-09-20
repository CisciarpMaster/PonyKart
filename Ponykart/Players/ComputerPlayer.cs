using PonykartParsers;

namespace Ponykart.Players {
    public class ComputerPlayer : Player {

        public ComputerPlayer(MuffinDefinition def, int id) : base(def, id) {

        }

        protected override void UseItem() {
            throw new System.NotImplementedException();
        }
    }
}
