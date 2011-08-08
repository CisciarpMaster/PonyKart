using System;

namespace Ponykart.Physics {
	/// <summary>
	/// Flags we use when firing trigger region events
	/// </summary>
	[Flags]
	public enum TriggerReportFlags {
		/// <summary>
		/// Not sure when you'd use "none", but oh well here it is
		/// </summary>
		None = 0,
		/// <summary>
		/// Something has entered the region
		/// </summary>
		Enter = 1,
		/// <summary>
		/// Something has left the region
		/// </summary>
		Leave = 2
	}

	static class TriggerReportFlagsExtensions {
		/// <summary>
		/// Helper method to see if this flag has an enter flag or not.
		/// </summary>
		public static bool IsEnterFlag(this TriggerReportFlags flags) {
			return (flags & TriggerReportFlags.Enter) == TriggerReportFlags.Enter;
		}

		/// <summary>
		/// Helper method to see if this flag has a leave flag or not.
		/// </summary>
		public static bool IsLeaveFlag(this TriggerReportFlags flags) {
			return (flags & TriggerReportFlags.Leave) == TriggerReportFlags.Leave;
		}
	}
}
