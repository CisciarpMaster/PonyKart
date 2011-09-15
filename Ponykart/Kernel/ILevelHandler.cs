
namespace Ponykart {
	public interface ILevelHandler {
		/// <summary>
		/// Since most handlers just hook up to events, this method should be used to detach from those events.
		/// If the handler also subclasses LDisposable, Detach() should call Dispose() and not the other way around.
		/// </summary>
		void Detach();
	}
}
