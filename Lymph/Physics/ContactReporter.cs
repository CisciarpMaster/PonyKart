using System;
using Mogre.PhysX;

namespace Ponykart.Phys {

	public delegate void ContactReportHandler(ContactPair pair, ContactPairFlags flags);

	/// <summary>
	/// Use AddEvent() to hook into an event, don't hook into them directly!
	/// </summary>
	public class ContactReporter : IUserContactReport
	{
		/// <summary>
		/// our 2D array of contact report delegates
		/// </summary>
		private ContactReportHandler[,] reporters;

		public ContactReporter() {
			reporters = new ContactReportHandler[7, 7];
		}

		/// <summary>
		/// Hooks up an event handler to a collision event.
		/// 
		/// The order of the two group IDs does not matter, as it will add them to both [a,b] and [b,a].
		/// 
		/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
		/// </summary>
		/// <param name="firstGroupID">One of the group IDs of the event you are listening for</param>
		/// <param name="secondGroupID">The other group ID of the event you are listening for</param>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void AddEvent(uint firstGroupID, uint secondGroupID, ContactReportHandler handler)
		{
			// check for invalid IDs
			if (firstGroupID >= 7)
				throw new ArgumentOutOfRangeException("firstGroupID", firstGroupID, "firstGroupID must be less than or equal to 6");
			if (secondGroupID >= 7)
				throw new ArgumentOutOfRangeException("secondGroupID", secondGroupID, "secondGroupID must be less than or equal to 6");

			reporters[firstGroupID, secondGroupID] += handler;
			reporters[secondGroupID, firstGroupID] += handler;
		}

		/// <summary>
		/// Removes a handler from a collision event. 
		/// 
		/// The order of the two group IDs does not matter, as it will remove from both [a,b] and [b,a].
		/// </summary>
		/// <param name="firstGroupID">One of the group IDs of the event you are listening for</param>
		/// <param name="secondGroupID">The other group ID of the event you are listening for</param>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void RemoveEvent(uint firstGroupID, uint secondGroupID, ContactReportHandler handler)
		{
			// check for invalid IDs
			if (firstGroupID >= 7)
				throw new ArgumentOutOfRangeException("firstGroupID", firstGroupID, "firstGroupID must be less than or equal to 6");
			if (secondGroupID >= 7)
				throw new ArgumentOutOfRangeException("secondGroupID", secondGroupID, "secondGroupID must be less than or equal to 6");

			reporters[firstGroupID, secondGroupID] -= handler;
			reporters[secondGroupID, firstGroupID] -= handler;
		}

		/// <summary>
		/// invoke an event.
		/// 
		/// note that it only invokes [a,b] and not [b,a] - if it invokes both, errors will happen
		/// </summary>
		private void FireEvent(ContactPair pair, ContactPairFlags flags)
		{
			var e = reporters[pair.ActorFirst.Group, pair.ActorSecond.Group];
			if (e != null) {
				e(pair, flags);
			}
		}

		/// <summary>
		/// Responds to the generic collision event physx gives us
		/// </summary>
		public void OnContactNotify(ContactPair pair, ContactPairFlags flags) {

			//System.Console.WriteLine("Collision: " + pair.ActorFirst.Name + " and " + pair.ActorSecond.Name + " - Flag: " + flags);

			uint ID1 = pair.ActorFirst.Group, ID2 = pair.ActorSecond.Group;

			if (ID1 < 7) { // check to make sure it's valid - don't need to check for negatives because it's a uint
				if (ID2 < 7) {
					FireEvent(pair, flags);
				} else
					throw new ArgumentOutOfRangeException("ID2", ID2, "[Contact Reporter] Actor 2 does not have a valid group ID! "
						+ pair.ActorSecond.Name + " (" + pair.ActorSecond.Group + ")");
			} else
				throw new ArgumentOutOfRangeException("ID1", ID1, "[Contact Reporter] Actor 1 does not have a valid group ID! "
					+ pair.ActorFirst.Name + " (" + pair.ActorFirst.Group + ")");
		}
	}
}
