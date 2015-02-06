namespace mWF
{
	public abstract class RequestBase<TStepOrdinal> where TStepOrdinal : struct
	{
		public TStepOrdinal? StateToStartFrom { get; internal set; }

		public bool WasError { get; internal set; }

		protected RequestBase(TStepOrdinal? stateToStartFrom)
		{
			StateToStartFrom = stateToStartFrom;
		}

		internal bool Continue(TStepOrdinal stateToStartFrom)
		{
			if(WasError)
				return false;
			if (StateToStartFrom.HasValue)
			{
				if (StateToStartFrom.Value.Equals(stateToStartFrom))
				{
					StateToStartFrom = null;
					return true;
				}
				return false;
			}
			return true;
		}
	}
}
