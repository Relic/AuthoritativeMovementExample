using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0,0,0.85]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private uint _ownerNetId;
		public event FieldEvent<uint> ownerNetIdChanged;
		public Interpolated<uint> ownerNetIdInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint ownerNetId
		{
			get { return _ownerNetId; }
			set
			{
				// Don't do anything if the value is the same
				if (_ownerNetId == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_ownerNetId = value;
				hasDirtyFields = true;
			}
		}

		public void SetownerNetIdDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_ownerNetId(ulong timestep)
		{
			if (ownerNetIdChanged != null) ownerNetIdChanged(_ownerNetId, timestep);
			if (fieldAltered != null) fieldAltered("ownerNetId", _ownerNetId, timestep);
		}
		private uint _frame;
		public event FieldEvent<uint> frameChanged;
		public Interpolated<uint> frameInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint frame
		{
			get { return _frame; }
			set
			{
				// Don't do anything if the value is the same
				if (_frame == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_frame = value;
				hasDirtyFields = true;
			}
		}

		public void SetframeDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_frame(ulong timestep)
		{
			if (frameChanged != null) frameChanged(_frame, timestep);
			if (fieldAltered != null) fieldAltered("frame", _frame, timestep);
		}
		private Vector3 _position;
		public event FieldEvent<Vector3> positionChanged;
		public InterpolateVector3 positionInterpolation = new InterpolateVector3() { LerpT = 0.85f, Enabled = true };
		public Vector3 position
		{
			get { return _position; }
			set
			{
				// Don't do anything if the value is the same
				if (_position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_position = value;
				hasDirtyFields = true;
			}
		}

		public void SetpositionDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_position(ulong timestep)
		{
			if (positionChanged != null) positionChanged(_position, timestep);
			if (fieldAltered != null) fieldAltered("position", _position, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			ownerNetIdInterpolation.current = ownerNetIdInterpolation.target;
			frameInterpolation.current = frameInterpolation.target;
			positionInterpolation.current = positionInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _ownerNetId);
			UnityObjectMapper.Instance.MapBytes(data, _frame);
			UnityObjectMapper.Instance.MapBytes(data, _position);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_ownerNetId = UnityObjectMapper.Instance.Map<uint>(payload);
			ownerNetIdInterpolation.current = _ownerNetId;
			ownerNetIdInterpolation.target = _ownerNetId;
			RunChange_ownerNetId(timestep);
			_frame = UnityObjectMapper.Instance.Map<uint>(payload);
			frameInterpolation.current = _frame;
			frameInterpolation.target = _frame;
			RunChange_frame(timestep);
			_position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			positionInterpolation.current = _position;
			positionInterpolation.target = _position;
			RunChange_position(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _ownerNetId);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _frame);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _position);

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (ownerNetIdInterpolation.Enabled)
				{
					ownerNetIdInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					ownerNetIdInterpolation.Timestep = timestep;
				}
				else
				{
					_ownerNetId = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_ownerNetId(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (frameInterpolation.Enabled)
				{
					frameInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					frameInterpolation.Timestep = timestep;
				}
				else
				{
					_frame = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_frame(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (positionInterpolation.Enabled)
				{
					positionInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					positionInterpolation.Timestep = timestep;
				}
				else
				{
					_position = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_position(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (ownerNetIdInterpolation.Enabled && !ownerNetIdInterpolation.current.UnityNear(ownerNetIdInterpolation.target, 0.0015f))
			{
				_ownerNetId = (uint)ownerNetIdInterpolation.Interpolate();
				//RunChange_ownerNetId(ownerNetIdInterpolation.Timestep);
			}
			if (frameInterpolation.Enabled && !frameInterpolation.current.UnityNear(frameInterpolation.target, 0.0015f))
			{
				_frame = (uint)frameInterpolation.Interpolate();
				//RunChange_frame(frameInterpolation.Timestep);
			}
			if (positionInterpolation.Enabled && !positionInterpolation.current.UnityNear(positionInterpolation.target, 0.0015f))
			{
				_position = (Vector3)positionInterpolation.Interpolate();
				//RunChange_position(positionInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
