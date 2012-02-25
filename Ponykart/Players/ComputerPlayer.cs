using Ponykart.Levels;

namespace Ponykart.Players {
    public class ComputerPlayer : Player {

        public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id) {

        }

        protected override void UseItem() {
            throw new System.NotImplementedException();
        }
    }
}
