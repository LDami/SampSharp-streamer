﻿// SampSharp.Streamer
// Copyright 2016 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.Definitions;
using SampSharp.Streamer.Events;

namespace SampSharp.Streamer.World
{
    public partial class DynamicObject : DynamicWorldObject<DynamicObject>, IGameObject
    {
        public DynamicObject(int modelid, Vector3 position, Vector3 rotation = new Vector3(), int worldid = -1,
            int interiorid = -1, BasePlayer player = null, float streamdistance = 200.0f, float drawdistance = 0.0f, DynamicArea area = null, int priority = 0)
        {
            Id = Internal.CreateDynamicObject(modelid, position.X, position.Y, position.Z, rotation.X, rotation.Y,
                rotation.Z, worldid, interiorid, player?.Id ?? -1, streamdistance, drawdistance, area?.Id ?? -1, priority);
        }

        public DynamicObject(int modelid, Vector3 position, Vector3 rotation, float streamdistance, int[] worlds = null,
            int[] interiors = null, BasePlayer[] players = null, float drawdistance = 0.0f, DynamicArea[] areas = null, int priority = 0)
        {
            if (worlds == null) worlds = new[] {-1};
            if (interiors == null) interiors = new[] {-1};
            var pl = players?.Select(p => p.Id).ToArray() ?? new[] {-1};
            var ar = areas?.Select(a => a.Id).ToArray() ?? new[] { -1};
            Id = Internal.CreateDynamicObjectEx(modelid, position.X, position.Y, position.Z, rotation.X, rotation.Y,
                rotation.Z, drawdistance, streamdistance, worlds, interiors, pl, ar, priority, worlds.Length, interiors.Length,
                pl.Length, ar.Length);
        }

        public override StreamType StreamType => StreamType.Object;

        public bool IsMoving => Internal.IsDynamicObjectMoving(Id);

        public bool IsValid => Internal.IsValidDynamicObject(Id);

        public int ModelId
        {
            get { return GetInteger(StreamerDataType.ModelId); }
            set { SetInteger(StreamerDataType.ModelId, value); }
        }

        public float DrawDistance
        {
            get { return GetFloat(StreamerDataType.DrawDistance); }
            set { SetFloat(StreamerDataType.DrawDistance, value); }
        }

        public Vector3 Rotation
        {
            get
            {
                float x, y, z;
                Internal.GetDynamicObjectRot(Id, out x, out y, out z);
                return new Vector3(x, y, z);
            }
            set { Internal.SetDynamicObjectRot(Id, value.X, value.Y, value.Z); }
        }

        public int Move(Vector3 position, float speed, Vector3 rotation)
        {
            return Internal.MoveDynamicObject(Id, position.X, position.Y, position.Z, speed, rotation.X,
                rotation.Y, rotation.Z);
        }

        public int Move(Vector3 position, float speed)
        {
            return Internal.MoveDynamicObject(Id, position.X, position.Y, position.Z, speed, -1000.0f, -1000.0f,
                -1000.0f);
        }

        public void Stop()
        {
            Internal.StopDynamicObject(Id);
        }

        public void SetMaterial(int materialindex, int modelid, string txdname, string texturename,
            Color materialcolor = new Color())
        {
            Internal.SetDynamicObjectMaterial(Id, materialindex, modelid, txdname, texturename,
                materialcolor.ToInteger(ColorFormat.ARGB));
        }

        public void SetMaterialText(int materialindex, string text,
            ObjectMaterialSize materialsize = ObjectMaterialSize.X256X128, string fontface = "Arial", int fontsize = 24,
            bool bold = true, Color fontcolor = new Color(), Color backcolor = new Color(),
            ObjectMaterialTextAlign textalignment = ObjectMaterialTextAlign.Center)
        {
            Internal.SetDynamicObjectMaterialText(Id, materialindex, text, (int) materialsize, fontface, fontsize, bold,
                fontcolor.ToInteger(ColorFormat.ARGB),
                backcolor.ToInteger(ColorFormat.ARGB), (int) textalignment);
        }

        public event EventHandler<EventArgs> Moved;

        public event EventHandler<PlayerSelectEventArgs> Selected;

        public event EventHandler<PlayerEditEventArgs> Edited;

        public event EventHandler<PlayerShootEventArgs> Shot;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Internal.DestroyDynamicObject(Id);
        }

        public void GetMaterial(int materialindex, out int modelid, out string txdname, out string texturename,
            out Color materialColor)
        {
            int holderMaterialColor;
            Internal.GetDynamicObjectMaterial(Id, materialindex, out modelid, out txdname, out texturename,
                out holderMaterialColor, 64, 64);

            materialColor = Color.FromInteger(holderMaterialColor, ColorFormat.ARGB);
        }

        public void GetMaterialText(int materialindex, out string text, out ObjectMaterialSize materialSize,
            out string fontface, out int fontsize, out bool bold, out Color fontcolor, out Color backcolor,
            out ObjectMaterialTextAlign textalignment)
        {
            int holderFontColor, holderBackColor;
            int holderMaterialSize;
            int holderTextalignment;
            Internal.GetDynamicObjectMaterialText(Id, materialindex, out text, out holderMaterialSize, out fontface,
                out fontsize, out bold, out holderFontColor, out holderBackColor, out holderTextalignment, 1024, 64);

            fontcolor = Color.FromInteger(holderFontColor, ColorFormat.ARGB);
            backcolor = Color.FromInteger(holderBackColor, ColorFormat.ARGB);
            materialSize = (ObjectMaterialSize) holderMaterialSize;
            textalignment = (ObjectMaterialTextAlign) holderTextalignment;
        }

        public bool IsMaterialUsed(int materialindex)
        {
            return Internal.IsDynamicObjectMaterialUsed(Id, materialindex);
        }

        public bool IsMaterialTextUsed(int materialindex)
        {
            return Internal.IsDynamicObjectMaterialTextUsed(Id, materialindex);
        }

        public virtual void Edit(BasePlayer player)
        {
            AssertNotDisposed();

            Internal.EditPlayerObject(player.Id, Id);
        }

        public static void Select(BasePlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            Internal.SelectObject(player.Id);
        }

        public virtual void AttachTo(BaseVehicle vehicle, Vector3 offset, Vector3 rotation)
        {
            AssertNotDisposed();

            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            Internal.AttachDynamicObjectToVehicle(Id, vehicle.Id, offset.X, offset.Y, offset.Z, rotation.X,
                rotation.Y, rotation.Z);
        }

        public virtual void SetNoCameraCollision()
        {
            AssertNotDisposed();

            Internal.SetDynamicObjectNoCameraCol(Id);
        }

	    public virtual bool GetNoCameraCollision()
	    {
		    AssertNotDisposed();

		    return Internal.GetDynamicObjectNoCameraCol(Id);
	    }

	    public virtual void AttachCameraToObject(BasePlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            AssertNotDisposed();

            Internal.AttachCameraToDynamicObject(player.Id, Id);
        }

        public static void ToggleAllItems(BasePlayer player, bool toggle, DynamicObject[] exceptions)
        {
            var ids = exceptions?.Select(e => e.Id).ToArray() ?? new[] { -1 };
            WorldInternal.ToggleAllItems(player?.Id ?? -1, (int)StreamType.Object, toggle, ids,
                ids.Length);
        }

        public virtual void OnMoved(EventArgs e)
        {
            Moved?.Invoke(this, e);
        }

        public virtual void OnSelected(PlayerSelectEventArgs e)
        {
            Selected?.Invoke(this, e);
        }

        public virtual void OnEdited(PlayerEditEventArgs e)
        {
            Edited?.Invoke(this, e);
        }

        public virtual void OnShot(PlayerShootEventArgs e)
        {
            Shot?.Invoke(this, e);
        }
    }
}
