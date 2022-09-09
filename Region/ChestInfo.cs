using System;
using System.Collections.Generic;
using Terraria;

namespace ServerSideCharacter.Region
{
	public class ChestManager
	{
		[Flags]
		public enum Pending
		{
			Protect = 1,
			DeProtect = 2,
			Public = 4,
			UnPublic = 8,
			AddFriend = 16,
			RemoveFriend = 32,
			Info = 64
		}
		public readonly ChestInfo[] ChestInfo = new ChestInfo[Main.chest.Length];
		private readonly Dictionary<int, Pending> _pendings = new Dictionary<int, Pending>();
		private readonly Dictionary<int, int> _friendPendings = new Dictionary<int, int>();
		public ChestManager Initialize()
		{
			for (int i = 0; i < Main.chest.Length; i++)
			{
				ChestInfo[i] = new ChestInfo();
			}
			return this;
		}
		private void SetFriendP(ServerPlayer player, ServerPlayer friend)
		{
			if (_friendPendings.ContainsKey(player.Uuid))
				if (friend == null)
					_friendPendings.Remove(player.Uuid);
				else
					_friendPendings[player.Uuid] = friend.Uuid;
			else if (friend != null)
				_friendPendings.Add(player.Uuid, friend.Uuid);
		}
		public ServerPlayer GetFriendP(ServerPlayer player)
		{
			int uuid = _friendPendings.ContainsKey(player.Uuid) ? _friendPendings[player.Uuid] : -1;
			return ServerPlayer.FindPlayer(uuid);

		}
		public void AddPending(ServerPlayer player, Pending pending, ServerPlayer friend = null)
		{

			if (_pendings.ContainsKey(player.Uuid))
				_pendings[player.Uuid] |= pending;
			else
				_pendings.Add(player.Uuid, pending);
			if (pending.HasFlag(Pending.AddFriend) || pending.HasFlag(Pending.RemoveFriend))
				SetFriendP(player, friend);
		}
		public void SetPendings(ServerPlayer player, Pending pending, ServerPlayer friend = null)
		{
			if (_pendings.ContainsKey(player.Uuid))
				_pendings[player.Uuid] = pending;
			else
				_pendings.Add(player.Uuid, pending);
			if (pending.HasFlag(Pending.AddFriend) || pending.HasFlag(Pending.RemoveFriend))
				SetFriendP(player, friend);

		}
		public void RemovePending(ServerPlayer player, Pending pending)
		{
			if (_pendings.ContainsKey(player.Uuid))
				_pendings[player.Uuid] &= ~pending;
			if (pending.HasFlag(Pending.AddFriend) || pending.HasFlag(Pending.RemoveFriend))
				SetFriendP(player, null);
		}
		public void AddFriend(int chestId, ServerPlayer friend)
		{
			ChestInfo[chestId].AddFriend(friend);
		}
		public void RemoveFriend(int chestId, ServerPlayer friend)
		{
			ChestInfo[chestId].RemoveFriend(friend);
		}
		public void RemoveAllPendings(ServerPlayer player)
		{
			if (_pendings.ContainsKey(player.Uuid))
				_pendings[player.Uuid] = new Pending();
			SetFriendP(player, null);
		}
		public Pending GetPendings(ServerPlayer player)
		{
			return _pendings.ContainsKey(player.Uuid) ? _pendings[player.Uuid] : new Pending();

		}
		public void SetOwner(int chestId, int ownerId, bool isPublic)
		{
			ChestInfo[chestId].OwnerId = ownerId;
			ChestInfo[chestId].IsPublic = isPublic;
		}

		public bool IsNull(int chestId)
		{
			var id = ChestInfo[chestId].OwnerId;
			return id == -1;
		}
		public bool IsOwner(int chestId, ServerPlayer player)
		{
			var id = ChestInfo[chestId].OwnerId;
			return id == player.Uuid || player.PermissionGroup.HasPermission("chest") && id != -1;
		}
		public bool IsPublic(int chestId)
		{
			var isPublic = ChestInfo[chestId].IsPublic;
			return isPublic;
		}
		public bool CanOpen(int chestId, ServerPlayer player)
		{
			var id = ChestInfo[chestId].OwnerId;
			var isPublic = ChestInfo[chestId].IsPublic;
			var friends = ChestInfo[chestId].Friends;
			return id == -1 || id == player.Uuid || player.PermissionGroup.HasPermission("chest") || isPublic || friends.Contains(player.Uuid);
		}
	}
	public class ChestInfo
	{
		private int _ownerId = -1;
		private bool _isPublic;
		private List<int> _friends = new List<int>();

		public void AddFriend(ServerPlayer player)
		{
			if (!_friends.Contains(player.Uuid) && _ownerId > -1 && player.Uuid != _ownerId)
				_friends.Add(player.Uuid);
		}
		public void RemoveFriend(ServerPlayer player)
		{
			if (_friends.Contains(player.Uuid) && _ownerId > -1)
				_friends.RemoveAll(id => id == player.Uuid);
		}
		public int OwnerId
		{
			get { return _ownerId; }
			set
			{
				if (value <= -1)
				{
					_isPublic = false;
					_friends.Clear();
					value = -1; //Just in case if value is < -1
				}
				_ownerId = value;
			}
		}
		public bool IsPublic
		{
			get { return _isPublic; }
			set { _isPublic = value; }
		}
		public List<int> Friends
		{
			get { return _friends; }
		}
	}
}
