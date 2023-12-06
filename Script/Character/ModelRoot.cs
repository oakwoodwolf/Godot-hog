/*
 * [ Sonic Onset Adventure]
 * Copyright (c) 2023 Regan "CKDEV" Green
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

// TODOS:
// - Rewrite animation code

using Godot;

namespace SonicGodot.Character
{
    public partial class ModelRoot : SonicGodot.ModelRoot
    {
        // Tilt bone class
        internal class TiltBone
        {
            // Skeleton and bone
            private Skeleton3D _skeletonNode;

            internal int _boneIdx;

            // Constructor
            public TiltBone(Skeleton3D skeleton_node, string bone_name)
            {
                // Get bone
                _skeletonNode = skeleton_node;
                _boneIdx = _skeletonNode.FindBone(bone_name);
            }

            // Tilt bone
            public void Tilt(Quaternion quat)
            {
                // Tilt bone pose
                Quaternion pose = _skeletonNode.GetBonePoseRotation(_boneIdx);
                pose *= quat;
                _skeletonNode.SetBonePoseRotation(_boneIdx, pose);
            }
        }

        // Root bone class
        internal class RootBone
        {
            // Skeleton and bone
            private Skeleton3D _skeletonNode;
            internal int _boneIdx;

            private Transform3D _offset;

            // Constructor
            public RootBone(Skeleton3D skeleton_node, string bone_name)
            {
                // Get bone
                _skeletonNode = skeleton_node;
                _boneIdx = _skeletonNode.FindBone(bone_name);

                // Get offset
                _offset = _skeletonNode.GetBoneRest(_boneIdx);
            }

            // Set root bone
            public void Shear(Transform3D transform)
            {
                // Set bone pose
                Transform3D pose = _offset * transform;
                _skeletonNode.SetBoneEnabled(_boneIdx, false);
                _skeletonNode.SetBoneRest(_boneIdx, pose);
            }
        }

        // Model nodes
        internal Animator _animationPlayer;
        internal Skeleton3D _skeletonNode;
        public string CurrentAnim;
        internal Util.Animation.SkeletonPose _restPose;

        internal System.Collections.Generic.Dictionary<string, Util.Animation.AnimationTrack> _animationTracks = new System.Collections.Generic.Dictionary<string, Util.Animation.AnimationTrack>();

        // Animation functions
        internal const double AnimBlendSlow = 0.4;
        internal const double AnimBlendFast = 0.12;

        public virtual void ClearAnimation() => _animationPlayer.ClearAnimation();
        public virtual void PlayAnimation(string name, double speed = 1.0f)
        {
            CurrentAnim = name;
            _animationPlayer.PlayAnimation(name, speed);
        }

        // Tilt state
        internal Util.Spring _tilt = new Util.Spring(3.0f, 0.0f);

        internal Transform3D _shear = Transform3D.Identity;
        internal const float ShearLerp = 0.4f;

        internal Vector3? _pointOfInterest = null;

        public virtual void SetTilt(float tilt)
        {
            // Set jumpball tilt target
            _tilt.Goal = tilt;
        }

        public virtual void SetShear(Transform3D transform)
        {
            // Lerp shear
            var basis = new Basis(
                _shear.Basis.Column0.Lerp(transform.Basis.Column0, ShearLerp),
                _shear.Basis.Column1.Lerp(transform.Basis.Column1, ShearLerp),
                _shear.Basis.Column2.Lerp(transform.Basis.Column2, ShearLerp)
            );
            Vector3 origin = _shear.Origin.Lerp(transform.Origin, ShearLerp);
            _shear = new Transform3D(basis, origin);
        }

        public virtual void SetPointOfInterest(Vector3? pointOfInterest)
        {
            // Set point of interest
            _pointOfInterest = pointOfInterest;
        }

        // Model root
        public override void _Ready()
        {
            // Setup base
            base._Ready();
            _animationPlayer = GetNode<Animator>("Model/AnimationPlayer");
        }

        public override void _Process(double delta)
        {
            // Update base
            base._Process(delta);

            // Update tilt
            _tilt.Step((float)delta);
        }
    }
}
