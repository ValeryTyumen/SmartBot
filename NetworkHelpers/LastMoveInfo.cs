namespace NetworkHelpers
{
	public class LastMoveInfo
	{
		public bool GameOver;
		public CellChange[] CellChanges;
		public PlayerStateChange[] PlayerStateChanges; // <id, new position, new hp>
	}
}