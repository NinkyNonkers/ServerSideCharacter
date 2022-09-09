namespace ServerSideCharacter
{
	public enum SscMessageType
	{
		SyncPlayerHealth,
		SyncPlayerMana,
		SyncPlayerBank,
		RequestSaveData,
		RequestRegister,
		RequestSetGroup,
		RequestItem,
		RequestAuth,
		ResetPassword,
		SendLoginPassword,
		SendTimeSet,
		HelpCommand,
		KillCommand,
		ListCommand,
		SummonCommand,
		TpHereCommand,
		ButcherCommand,
		BanItemCommand,
		TpCommand,
		TimeCommand,
		ToggleExpert,
		ToggleHardMode,
		ToggleXmas,
		RegionCreateCommand,
		RegionRemoveCommand,
		RegionShareCommand,
		LockPlayer,
		TeleportPalyer,
		ToggleGodMode,
		SetGodMode,
		ServerSideCharacter,
		GenResources,
		ChestCommand,
		TpProtect
	}

	public enum GenerationType
	{
		Tree,
		Chest,
		Ore,
		Trap
	}
}
